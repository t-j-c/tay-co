*This is the third and final part of a series on creating and using Web API tests in .NET Core. Part One gives an overview of the application and dives in to creating the tests and establishing a performance benchmark. In Part Two, we utilize the tests to measure performance impact after changing our DB provider to Azure Cosmos DB. And in Part Three, we address performance degradation by decorating a Redis cache layer in our API.*

---

In Part Two, we were able to swap out our original PostgreSQL persistence implementation with a shiny, new Azure Cosmos DB implementation. In doing this, our suite of API performance/load tests began to fail. This pointed out that our new persistence strategy actually caused a degradation of performance in our API.

As I mentioned in the last post, this was most likely due to the fact that I was:

1. using the local Cosmos DB emulator, which has [limitations compared to the full-on cloud service](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator#differences-between-the-emulator-and-the-service). And,

1. not implementing the Cosmos DB repository with [performance](https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips) as a main priority.

In this third and final part of the “Testing a .NET Core Web API” series, we’re going to attempt to make improvements and get our tests passing again by adding in a Redis Cache repository using the decorator pattern.

![](https://cdn-images-1.medium.com/max/10214/1*wKlao_64cFXa0ttSm7v7Ug.jpeg)

---

### Adding a Decorator Repository

To do this, we can first create a new project that will have the sole responsibility of interacting with Redis:

![](https://cdn-images-1.medium.com/max/2000/1*a2RMWy3j6Uu8EVExe3j-LQ.png)

To apply the [decorator pattern](https://en.wikipedia.org/wiki/Decorator_pattern#Structure), the `CookbookRedisRepository` is going to implement the same `ICookbookRepository` that our `CookbookCosmosDbRepository` implements. At first, we can just have this be a pass through without adding any new behavior:

```csharp
public class CookbookRedisRepository : ICookbookRepository
{
    private readonly ICookbookRepository _parentRepository;
    public CookbookRedisRepository(ICookbookRepository parentRepository)
    {
        _parentRepository = parentRepository;
    }

    public void Add(string name)
    {
        _parentRepository.Add(name);
    }

    public int Count()
    {
        return _parentRepository.Count();
    }

    public void Delete()
    {
        _parentRepository.Delete();
    }

    public ICollection<Cookbook> Get()
    {
        return _parentRepository.Get();
    }

    public Cookbook Get(string name)
    {
        return _parentRepository.Get(name);
    }
}
```

The Dependency Injection container provided out of the box by .NET Core doesn’t support the Decorator pattern. So we’ll be using [Scrutor](https://github.com/khellang/Scrutor) to provide us with a rather simple way to register our new decorator repository. Now, inside of `Startup.cs` we have:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ICookbookRepository, CookbookCosmosDbRepository>();
    services.Decorate<ICookbookRepository, CookbookRedisRepository>();

    services.AddMvc();
}
```

With our new decorator repository implemented and registered, we can debug a request to make sure that the control flow is as expected:

<figure>
  <img src="https://cdn-images-1.medium.com/max/2000/1*HwFvY_Q2KRxr8m9MNnfrGA.png"/>
  <figcaption>1. In our Controller, about to enter <code>ICookbookRepository</code>. According to our registration, we should first go into <code>CookbookRedisRepository</code>.</figcaption>
</figure>

<figure>
  <img src="https://cdn-images-1.medium.com/max/2000/1*1VY6AovNxzFRzs4aMCOMuA.png"/>
  <figcaption>2. Good so far! Next we should pass through into <code>CookbookCosmosDbRepository</code>.</figcaption>
</figure>

<figure>
  <img src="https://cdn-images-1.medium.com/max/2000/1*qcaEc5yXq7kKZuNxTRC_qA.png"/>
  <figcaption>3. Decorator successful.</figcaption>
</figure>

Now that we have our repository decorated and running, we can append the Redis behavior into our decorator and hopefully increase the performance of our API.

---

### Connecting to Redis Cache

After [installing Redis locally](https://github.com/MicrosoftArchive/redis/tree/win-3.2.100), we can update our `CookbookRedisRepository` to interact with the cache:

```csharp
public class CookbookRedisRepository : ICookbookRepository
{
    private readonly ICookbookRepository _parentRepository;
    private readonly IDatabase _cache;
    public CookbookRedisRepository(ICookbookRepository parentRepository, IConfiguration configuration)
    {
        _parentRepository = parentRepository;
        _cache = ConnectionMultiplexer.Connect(configuration["CookBook.Redis:Connection"]).GetDatabase();
    }

    public void Add(string name)
    {
        _parentRepository.Add(name);
    }

    public int Count()
    {
        return _parentRepository.Count();
    }

    public void Delete()
    {
        _cache.Execute("FLUSHALL");
        _parentRepository.Delete();
    }

    public ICollection<Cookbook> Get()
    {
        return _parentRepository.Get();
    }

    public Cookbook Get(string name)
    {
        var cacheResult = _cache.StringGet(name);
        if (!cacheResult.IsNullOrEmpty) return JsonConvert.DeserializeObject<Cookbook>(cacheResult);

        var result = _parentRepository.Get(name);
        _cache.StringSet(name, JsonConvert.SerializeObject(result));
        return result;
    }
}
```

When our tests run now, the first call to the Get endpoint will attempt to find a result in the Cache. Since the cache is empty at this point, it will continue on to find a result from the parent repository (in this case the Cosmos DB repo). Once it finds that, it then sets that result into the cache so that any subsequent calls for the same resource will be retrieved from Redis, without even having to interact with Cosmos DB.

With this implementation, our tests are all passing again! I even went ahead and changed the expected value to a lower number that will force them to fail in order to see what the actual values were:

![](https://cdn-images-1.medium.com/max/2000/1*KfSAFX0qvXMdAdY1Xeuvjg.png)

![](https://cdn-images-1.medium.com/max/2000/1*dsxU89PinvpNuTvNe9GWrw.png)

![](https://cdn-images-1.medium.com/max/2000/1*y500JYg1RcvpIdp2shoCTQ.png)

![](https://cdn-images-1.medium.com/max/2000/1*JhC9xzhgQ6-Gn4mpAY3NiQ.png)

Ultimately, after implementing the cache repository, we’ve reduced the latency of our API by over 50%. We can now update the expected value in our tests given these new results and continue to confidently make changes to our API, knowing that any performance degradation will likely make itself visible through a failing test.
