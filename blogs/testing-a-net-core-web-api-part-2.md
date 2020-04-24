*This is part two of three in a series on creating and using Web API tests in .NET Core. Part One gives an overview of the application and dives in to creating the tests and establishing a performance benchmark. In Part Two, we utilize the tests to measure performance impact after changing our DB provider to Azure Cosmos DB. And in Part Three, we address performance degradation by decorating a Redis cache layer in our API.*

---

![](https://cdn-images-1.medium.com/max/2160/1*0pcQu4kfwJJUDNQP1eVwIQ.jpeg)

But before we do that…

We’ll need to revisit our API and the test that we created in Part 1. Currently, our API doesn’t really interact with any data. It just returns the count from a table in PostgreSQL. But we haven’t actually populated any data in that table. In order to get a more realistic performance measurement, we can update our test to create some test data for our API:

```csharp
private const string _cookbookNamePrefix = "PerformanceTestCookbook";

[Fact]
public void AverageResponseTime_100Requests_1MaxParallel()
{
    // Arrange
    CookbookTestDataSetup.CreateCookbooks(_cookbookNamePrefix, 1000);

    // Act
    var results = HttpHelper.ExecuteParallelRequests($"{CookbookTestDataSetup.CookbookApiUrl}/{_cookbookNamePrefix}_1", 100, 1);

    // Assert
    Assert.InRange(results.AverageTimeInMilliseconds, 0, 6);
}
```

The Arrange step of the test is now adding 1000 records to the table which is queried in the endpoint that we then call in the Act step. (Note: The endpoint is still being called 100 times, I just moved the `HttpClient` code into a helper class that will come in handy in a later post.)

Now we can update the API to contain all of the endpoints required by the test:

```csharp
[Route("api/cookbook")]
public class CookbookController : Controller
{
    private readonly ICookbookRepository _cookbookRepository;
    public CookbookController(ICookbookRepository cookbookRepository)
    {
        _cookbookRepository = cookbookRepository;
    }

    [HttpGet]
    [Route("{name}")]
    [ProducesResponseType(200, Type = typeof(CookbookDto))]
    [ProducesResponseType(404)]
    public IActionResult Get(string name)
    {
        var result = _cookbookRepository.Get(name);
        if (result == null) return NotFound();

        return Ok(Map(result));
    }

    [HttpPost]
    [ProducesResponseType(200)]
    public IActionResult Add([FromBody] AddCookbookCommand message)
    {
        _cookbookRepository.Add(message.Name);

        return Ok();
    }

    [HttpDelete]
    [Route("all")]
    [ProducesResponseType(200)]
    public IActionResult Delete()
    {
        _cookbookRepository.Delete();

        return Ok();
    }

    private static CookbookDto Map(Cookbook model)
    {
        return new CookbookDto
        {
            Id = model.CookbookId,
            Name = model.Name,
            Recipes = model.Recipes?.Select(Map).ToList() ?? new List<RecipeDto>()
        };
    }

    private static RecipeDto Map(Recipe model)
    {
        return new RecipeDto
        {
            Id = model.RecipeId,
            Name = model.Title,
            CookbookId = model.CookbookId
        };
    }
}
```

One thing worth pointing out here is that this controller no longer depends directly on the PostgreSQL `DbContext`. It now depends on a simple repository interface:

```csharp
public interface ICookbookRepository
{
    int Count();
    ICollection<Cookbook> Get();
    Cookbook Get(string name);
    void Add(string name);
    void Delete();
}
```

With this approach, the implementation of this interface can easily be swapped out with any other provider (e.g. MySQL, in-memory storage, a Word document). And as long as the given provider satisfies this interface, then our controller doesn’t need to change.

In this instance, we’re going to be swapping our PostgreSQL provider with a new Azure Cosmos DB provider.

![](https://cdn-images-1.medium.com/max/9792/1*-W4_ED_6hxva6-cJSpMkXw.jpeg)

---

## To the cloud!

Kind of. We’re actually going to be using the [Azure Cosmos DB emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator#installation). This allows us to write our code exactly the same as if we were in the cloud, but are actually just bumping up against a local instance of Cosmos DB.

With the emulator up and running, we can now provide a new repository that implements our ICookbookRepository interface. This provider will be using the [Microsoft Azure Cosmos DB Client library](https://www.nuget.org/packages/Microsoft.Azure.DocumentDB/) which gives us everything we need to interact with our local Cosmos DB instance:

```csharp
public class CookbookCosmosDbRepository : ICookbookRepository
{
    private readonly DocumentClient _client;

    private readonly Uri _databaseUri;
    private readonly Uri _collectionUri;

    public CookbookCosmosDbRepository()
    {
        _client = new DocumentClient(
            new Uri("https://localhost:8081"), 
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

        const string databaseName = "CookBookDB";
        _databaseUri = UriFactory.CreateDatabaseUri(databaseName);
        const string collectionName = "CookBookCollection";
        _collectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);

        _client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName }).GetAwaiter().GetResult();
        _client.CreateDocumentCollectionIfNotExistsAsync(_databaseUri, 
            new DocumentCollection { Id = collectionName }).GetAwaiter().GetResult();
    }

    public void Add(string name)
    {
        var newCookbook = new Cookbook
        {
            Name = name
        };

        _client.CreateDocumentAsync(_collectionUri, newCookbook).GetAwaiter().GetResult();
    }

    public int Count()
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        _client.DeleteDocumentCollectionAsync(_collectionUri).GetAwaiter().GetResult();
        _client.CreateDocumentCollectionIfNotExistsAsync(_databaseUri,
            new DocumentCollection { Id = "CookBookCollection" }).GetAwaiter().GetResult();
    }

    public ICollection<Cookbook> Get()
    {
        var results = _client.CreateDocumentQuery<Cookbook>(_collectionUri);
        return results.ToList();
    }

    public Cookbook Get(string name)
    {
        var results = _client.CreateDocumentQuery<Cookbook>(_collectionUri);
        return results.Where(c => c.Name == name).ToList().SingleOrDefault();
    }
}
```

And since our controller only depends on the interface, all we need to do now is swap out our interface provider in Startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var connStr = Configuration.GetConnectionString("CookbookDatabase_PostgreSQL");
    services.AddDbContext<CookbookContext_PostgreSQL>(opt => opt.UseNpgsql(connStr));

    services.AddTransient<ICookbookRepository, CookbookPostgresRepository>();

    services.AddMvc();
}
```

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ICookbookRepository, CookbookCosmosDbRepository>();

    services.AddMvc();
}
```

We were able to easily “plug in” a new data provider without directly affecting the controller since we’re still adhering to the `ICookbookRepository` interface that the controller depends on. This is an example of a Plugin Architecture (as outlined in Uncle Bob’s [Clean Architecture](https://amzn.to/2EMcoPW) book):
> The history of software development technology is the story of how to conveniently create plugins to establish a scalable and maintainable system architecture.

And with that, we can build and run our application and run our test to measure the impact:

    Message: Assert.InRange() Failure
    Range: (0–6)
    Actual: 8.48414

    Message: Assert.InRange() Failure
    Range: (0–6)
    Actual: 6.545263

    Message: Assert.InRange() Failure
    Range: (0–6)
    Actual: 6.426295

With this, we can see that there seems to be some minor performance degradation in our endpoint after the change. There are most likely tweaks that can be made in our repository to increase the performance (aka I am probably not using the Cosmos DB API in the most efficient way). But most importantly, we are aware of the impact being made and can react accordingly before releasing this change.

If you’d like to see more of the code shown in this post, you can find it [here](https://github.com/t-j-c/cookbook-load-testing).
