Performance is often one of the key focus points when building enterprise software. Many of the systems that we build rely heavily on communications with other systems. When these external communications become slow, then our software becomes slow. Unfortunately, we often have no control over the response time of the services that we depend on. However, we can optimize the way that we communicate with those services in order to ensure maximum performance.

![](https://cdn-images-1.medium.com/max/7892/1*5AR28T0O-z8CiKMzlcW1Lg.jpeg)

---

## Running in parallel

One of the best ways to optimize this type of software in C#, is by utilizing the [Task Parallel Library](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) (aka TPL). If you’re not familiar, this library provides a number of APIs that allow you to execute your code in parallel. This becomes especially valuable in the case mentioned previously, where we have some process that relies on multiple external communications.

---

### Managing Complexity

When working with a large number of tasks that return different types, using the TPL can get complicated rather quickly. A common case can be seen as follows. Let’s say we have the following few methods that do some potentially long-running communications:

```csharp
static async Task SimulateWorkAsync()
{
	await Task.Delay(1000);
}

static async Task<long> SimulateLongWorkAsync()
{
	var stopwatch = Stopwatch.StartNew();
	await Task.Delay(5000);
	return stopwatch.ElapsedMilliseconds;
}

static async Task<HttpStatusCode> CheckHttpStatusAsync(string url)
{
	var response = await httpClient.GetAsync(url);\
	return response.StatusCode;
}
```

One simple way to execute these methods in parallel is by using [Task.WhenAll](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall?view=netcore-3.0#System_Threading_Tasks_Task_WhenAll_System_Threading_Tasks_Task___):

```csharp
await Task.WhenAll(
	SimulateWorkAsync(),
	SimulateLongWorkAsync(),
	CheckHttpStatusAsync("http://www.google.com"),
	CheckHttpStatusAsync("http://www.facebook.com"),
	CheckHttpStatusAsync("http://slowwly.robertomurray.co.uk/delay/3000/url/http://www.microsoft.com")
);
```
> Side note: As seen used above, I’ve found [Slowwly](http://slowwly.robertomurray.co.uk/) to be a great website to simulate slow HTTP requests. It allows you to specify a delayed response directly in the URL.

While this does accomplish our goal of running the methods in parallel, what if we need to use the return value of these methods? One common way of achieving this is by keeping each Task in it’s own variable, so that the `.Result` can be accessed later:

```csharp
var workTask = SimulateWorkAsync();
var longWorkTask = SimulateLongWorkAsync();
var googleTask = CheckHttpStatusAsync("http://www.google.com");
var facebookTask = CheckHttpStatusAsync("http://www.facebook.com");
var microsoftTask = CheckHttpStatusAsync("http://slowwly.robertomurray.co.uk/delay/3000/url/http://www.microsoft.com");

await Task.WhenAll(
	workTask,
	longWorkTask,
	googleTask,
	facebookTask,
	microsoftTask
	);
```

As you can see, this approach can easily get complex if you start to add more methods. One thing we can do is use [`Task.ContinueWith`](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.continuewith?view=netcore-3.0) to handle the results of the tasks as continuations:

```csharp
await Task.WhenAll(
	SimulateWorkAsync(),
	SimulateLongWorkAsync().ContinueWith(cont =>
		Console.WriteLine($"{nameof(SimulateLongWorkAsync)} took {cont.Result} ms.")),
	CheckHttpStatusAsync("http://www.google.com").ContinueWith(cont =>
		Console.WriteLine($"http://www.google.com returned {cont.Result}")),
	CheckHttpStatusAsync("http://www.facebook.com").ContinueWith(cont =>
		Console.WriteLine($"http://www.facebook.com returned {cont.Result}")),
	CheckHttpStatusAsync("http://slowwly.robertomurray.co.uk/delay/3000/url/http://www.microsoft.com").ContinueWith(cont =>
		Console.WriteLine($"http://slowwly.robertomurray.co.uk/delay/3000/url/http://www.microsoft.com returned {cont.Result}"))
	);
```

With this, we can see that the calls to `CheckHttpStatusAsync` are essentially the same (aside from the URL that’s passed in). We should be able to consolidate this even further by using LINQ to execute that method against a collection of URLs:

```csharp
var urls = new[]
{
	"http://www.google.com",
	"http://www.facebook.com",
	"http://slowwly.robertomurray.co.uk/delay/3000/url/http://www.microsoft.com"
};

await Task.WhenAll(
	SimulateWorkAsync(),
	SimulateLongWorkAsync().ContinueWith(cont =>
		Console.WriteLine($"{nameof(SimulateLongWorkAsync)} took {cont.Result} ms.")),
	urls.Select(url => CheckHttpStatusAsync(url).ContinueWith(cont =>
		Console.WriteLine($"{url} returned {cont.Result}")))
	);
```

However, this doesn’t work since we’re now passing different types (`Task` and `IEnumerable<Task>`) into `Task.WhenAll`. We can solve this problem by using the `Enumerable` static methods to build a single `IEnumerable<Task>` that we can pass into `Task.WhenAll`:

```csharp
var urls = new[]
{
	"http://www.google.com",
	"http://www.facebook.com",
	"http://slowwly.robertomurray.co.uk/delay/3000/url/http://www.microsoft.com"
};

await Task.WhenAll(Enumerable.Empty<Task>()
	.Append(SimulateWorkAsync())
	.Append(SimulateLongWorkAsync().ContinueWith(cont =>
		Console.WriteLine($"{nameof(SimulateLongWorkAsync)} took {cont.Result} ms.")))
	.Concat(urls.Select(url => CheckHttpStatusAsync(url).ContinueWith(cont =>
		Console.WriteLine($"{url} returned {cont.Result}"))))
	);
```

---

## Conclusion

We’ve managed to find a way to execute a variety of tasks in parallel with a single statement. This approach is neat and concise, but more importantly it achieves our original goal of optimizing our software to handle the poor performance of others as best as we can.
