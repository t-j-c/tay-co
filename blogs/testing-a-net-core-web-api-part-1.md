*This is part one of three in a series on creating and using Web API tests in .NET Core. Part One gives an overview of the application and dives in to creating the tests and establishing a performance benchmark. In Part Two, we utilize the tests to measure performance impact after changing our DB provider to Azure Cosmos DB. And in Part Three, we address performance degradation by decorating a Redis cache layer in our API.*

---

![](https://cdn-images-1.medium.com/max/3840/1*j4wfpB40AyyJkhTAsvWcXg.jpeg)

Recently at work, my team created a set of Performance/Load tests around a Web API endpoint (in .NET Framework). We did this as a means of being able to measure the relative impact on performance that we may have been introducing to our system after making a change to the code base.

I’ve been wanting to introduce myself to .NET Core for a while now. And after going through the above exercise at work, I decided that I would spin up a .NET Core app and try out some of these concepts.

---

## The Application

I’m going to skip going over most of the boiler plate code, since the focus of this will be around the tests. I’ll instead post some links to resources that I used to set up the app on my laptop (running Ubuntu 16.04) . I’ll also leave a link to the GitHub repository where all of the code and tests that I used will live:

* [.NET Core Installation](https://www.microsoft.com/net/core)

* [ASP.NET Core Web API Setup](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-vsc)

* [EF Core PostgreSQL Setup](https://docs.microsoft.com/en-us/ef/core/providers/npgsql/) (more on why I chose PostgreSQL as a DB Provider later on)

* [GitHub repository](https://github.com/t-j-c/cookbook-load-testing)

Here’s a snippet of the Web API endpoint that we will be hitting up against in our tests:

```csharp
[Route("api/[controller]")]
public class CookbookController : Controller
{
    private readonly CookbookContext_PostgreSQL _db;
    public CookbookController(CookbookContext_PostgreSQL db)
    {
        _db = db;
    }

    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] 
        { 
            $"CookBooks: {_db.Cookbooks.Count()}"
        };
    }
}
```

This endpoint simply returns a Count from one of the tables in our PostgreSQL database. Again, the focus of this is on the tests and not on any software design patterns or best practices. Take the actual code with a grain of salt. And speaking of the tests…

---

## The Tests

In order to run any sort of test against our API, it needs to be up and running. Navigating to the API directory from the command line and running dotnet run does the trick, giving the following output:

    ~/Workspaces/CookBook/CookBook.API$ dotnet run
    Hosting environment: Production
    Content root path: /home/taylor/Workspaces/CookBook/CookBook.API
    Now listening on: [http://localhost:5000](http://localhost:5000)
    Application started. Press Ctrl+C to shut down.

Now that the app is running (on localhost:5000), we need a test to run against it. I’m going to use a simple [XUnit test project](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) to serve as a home for our performance tests. One thing to point out is that our tests are only going to be asserting the response time of the endpoint, despite many [other variables/metrics](https://www.thoughtworks.com/insights/blog/performance-testing-nutshell) that could be considered in a performance test. This holds true to the One Assertion Per Test principle (as outlined in Jay Fields’ [Working Effectively With Unit Tests](https://amzn.to/2EMoV5T)):
> Tests with a tight focus on one behavior of the system are almost always easier to write today, and easier to read tomorrow.

Finally, here is the first pass at our performance test:

```csharp
[Fact]
public void RequestTime()
{
    DateTime start;
    DateTime end;
    using (var client = new HttpClient())
    {
        start = DateTime.Now;
        var response = client.GetAsync("http://localhost:5000/api/cookbook").Result;
        end = DateTime.Now;
    }

    var expected = 1;
    var actual = (int)(end - start).TotalMilliseconds;
    Assert.True(actual <= expected, $"Expected total milliseconds of less than or equal to {expected} but was {actual}.");
}   
```

Note that our expected value is 1. Now I don’t actually expect the endpoint to perform in 1 millisecond or less. I actually expect the test to fail. Mainly so that it spits out the message passed into Assert.True which will tell us the *actual* time it took. Eventually, we’ll need to determine what this expected value should be since it will essentially be our benchmark for the response time of this endpoint.

Time to run our test. Here is the command I’m using to run it:

    dotnet test --filter “FullyQualifiedName=CookBook.Test.CookbookApiPerformanceTest.RequestTime”

And here is the error message from the output of our first test run:

`Expected total milliseconds of less than or equal to 1 but was 1902.`

Almost 2 seconds for hitting an endpoint that does virtually nothing seems a bit much. That’s probably just because there’s some extra time when accessing the endpoint for the first time. Let’s try it a few more times:

1. `Expected total milliseconds of less than or equal to 1 but was 36.`

1. `Expected total milliseconds of less than or equal to 1 but was 91.`

1. `Expected total milliseconds of less than or equal to 1 but was 31.`

There’s clearly some fluctuation happening here. This is one of the things that we quickly came to notice when running similar tests at my workplace. The response time from hitting an endpoint can change drastically depending on a seemingly infinite number of variables. To help reduce the fragility of this sort of test, we decided on a few things:

* The benchmark for the response time of the endpoint would be respective to our local development environments only.

* The benchmark would be established by using an average response time against multiple machines.

For the test that we are running, the first point above is irrelevant since I don’t plan on publishing/deploying the application to any other environments. However, we will utilize the second point. Here is the updated test to use an *average* response time from 100 requests:

```csharp
[Fact]
public void AverageResponseTime_100Requests()
{
    var allResponseTimes = new List<(DateTime Start, DateTime End)>();

    for (var i = 0; i < 100; i++)
    {
        using (var client = new HttpClient())
        {
            var start = DateTime.Now;
            var response = client.GetAsync("http://localhost:5000/api/cookbook").Result;
            var end = DateTime.Now;

            allResponseTimes.Add((start, end));
        }
    }

    var expected = 1;
    var actual = (int)allResponseTimes.Select(r => (r.End - r.Start).TotalMilliseconds).Average();
    Assert.True(actual <= expected, $"Expected average response time of less than or equal to {expected} ms but was {actual} ms.");
}
```

And here are the results from running the new test a few times:

1) `Expected average response time of less than or equal to 1 ms but was 3 ms.`
2) `Expected average response time of less than or equal to 1 ms but was 6 ms.`
3) `Expected average response time of less than or equal to 1 ms but was 3 ms.`

Now that we have some more consistent results, we can establish a baseline. Let’s make it the average of the above 3 runs times two, which comes out to 8. Now our test passes and even more importantly, we have some benchmark measurement of how our endpoint is currently performing.
