using Xunit.Abstractions;

namespace Test;

public class CachingTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly SampleProductController _controller;
    private readonly IMemoryCache _cache;
    private readonly HttpContext _httpContext;

    public CachingTests(ITestOutputHelper output)
    {
        _output = output;
        TestOutputHelper.Initialize(output);

        // Set up cache
        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();
        _cache = serviceProvider.GetRequiredService<IMemoryCache>();

        // Set up HTTP context
        _httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Request =
            {
                Path = "/SampleProduct"
            }
        };

        // Set up controller
        var repository = new SampleProductRepository(_context);
        _controller = new SampleProductController(repository);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };

        // Seed test data
        var product = new SampleProduct
        {
            ProductId = 1,
            Name = "Test Product",
            Price = 19.99m,
            Description = "Test Description"
        };
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetProducts_CachesResponse()
    {
        _output.WriteLine("\nTesting: Caching of Get All Products");
        _output.WriteLine("Checking if the response is cached when getting all products");
        // Arrange
        var cacheAttribute = new CacheAttribute();
        _httpContext.Request.Path = "/SampleProduct";
        var actionContext = new ActionContext(_httpContext, new RouteData(), new ActionDescriptor());
        var executingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), _controller);
        var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), _controller);

        // Act 1 - First request
        var actionResult = await _controller.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        executedContext.Result = okResult;
        
        await cacheAttribute.OnActionExecutionAsync(executingContext, async () => executedContext);

        // Get the cache key
        var cacheKey = GetCacheKey(executingContext);
        var firstResponse = executedContext.Result as ObjectResult;

        // Act 2 - Second request (should be from cache)
        var cachedValue = _cache.Get(cacheKey);

        // Assert
        Assert.NotNull(cachedValue);
        Assert.Equal((firstResponse?.Value as IEnumerable<SampleProduct>)?.First().Name, 
                    (cachedValue as IEnumerable<SampleProduct>)?.First().Name);
    }

    [Fact]
    public async Task GetProduct_CachesResponse()
    {
        _output.WriteLine("\nTesting: Caching of Get Single Product");
        _output.WriteLine("Checking if the response is cached when getting a single product");
        // Arrange
        var cacheAttribute = new CacheAttribute();
        _httpContext.Request.Path = "/SampleProduct/1";
        var actionContext = new ActionContext(_httpContext, new RouteData(), new ActionDescriptor());
        var executingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), 
            new Dictionary<string, object> { { "id", 1 } }, _controller);
        var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), _controller);

        // Act 1 - First request
        var actionResult = await _controller.GetById(1);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        executedContext.Result = okResult;
        
        await cacheAttribute.OnActionExecutionAsync(executingContext, async () => executedContext);

        // Get the cache key
        var cacheKey = GetCacheKey(executingContext);
        var firstResponse = executedContext.Result as ObjectResult;

        // Act 2 - Second request (should be from cache)
        var cachedValue = _cache.Get(cacheKey);

        // Assert
        Assert.NotNull(cachedValue);
        Assert.Equal((firstResponse?.Value as SampleProduct)?.Name, 
                    (cachedValue as SampleProduct)?.Name);
    }

    [Fact]
    public async Task GetProduct_DifferentIds_DifferentCacheKeys()
    {
        // Arrange
        var cacheAttribute = new CacheAttribute();
        
        // First request context
        _httpContext.Request.Path = "/SampleProduct/1";
        var actionContext1 = new ActionContext(_httpContext, new RouteData(), new ActionDescriptor());
        var executingContext1 = new ActionExecutingContext(actionContext1, new List<IFilterMetadata>(), 
            new Dictionary<string, object> { { "id", 1 } }, _controller);
        
        // Second request context
        _httpContext.Request.Path = "/SampleProduct/2";
        var actionContext2 = new ActionContext(_httpContext, new RouteData(), new ActionDescriptor());
        var executingContext2 = new ActionExecutingContext(actionContext2, new List<IFilterMetadata>(), 
            new Dictionary<string, object> { { "id", 2 } }, _controller);

        // Act
        var cacheKey1 = GetCacheKey(executingContext1);
        var cacheKey2 = GetCacheKey(executingContext2);

        // Assert
        Assert.NotEqual(cacheKey1, cacheKey2);
    }

    private static string GetCacheKey(ActionExecutingContext context)
    {
        var keyBuilder = new System.Text.StringBuilder();
        keyBuilder.Append($"{context.HttpContext.Request.Path}");

        foreach (var (key, value) in context.ActionArguments.OrderBy(a => a.Key))
        {
            keyBuilder.Append($"|{key}={value}");
        }

        return keyBuilder.ToString();
    }
}
