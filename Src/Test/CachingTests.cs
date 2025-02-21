using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Api.Controllers;
using Api.Data;
using Api.Models;
using Api.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Test;

public class CachingTests
{
    private readonly SampleDbContext _context;
    private readonly SampleProductController _controller;
    private readonly IMemoryCache _cache;
    private readonly HttpContext _httpContext;

    public CachingTests()
    {
        // Set up database
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new SampleDbContext(options);

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
        _controller = new SampleProductController(_context);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };

        // Seed test data
        var product = new SampleProduct
        {
            Id = 1,
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
        // Arrange
        var cacheAttribute = new CacheAttribute();
        _httpContext.Request.Path = "/SampleProduct";
        var actionContext = new ActionContext(_httpContext, new RouteData(), new ActionDescriptor());
        var executingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), _controller);
        var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), _controller);

        // Act 1 - First request
        var actionResult = await _controller.GetProducts();
        executedContext.Result = new OkObjectResult(actionResult.Value);
        
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
        // Arrange
        var cacheAttribute = new CacheAttribute();
        _httpContext.Request.Path = "/SampleProduct/1";
        var actionContext = new ActionContext(_httpContext, new RouteData(), new ActionDescriptor());
        var executingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), 
            new Dictionary<string, object> { { "id", 1 } }, _controller);
        var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), _controller);

        // Act 1 - First request
        var actionResult = await _controller.GetProduct(1);
        executedContext.Result = new OkObjectResult(actionResult.Value);
        
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
