namespace Api.Infrastructure.Caching;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CacheAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _timeToLiveSeconds;
    private readonly ResponseCacheType _cacheType;

    public CacheAttribute(int timeToLiveSeconds = 300, ResponseCacheType cacheType = ResponseCacheType.Private)
    {
        _timeToLiveSeconds = timeToLiveSeconds;
        _cacheType = cacheType;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheKey = GenerateCacheKeyFromRequest(context);
        var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

        // Try to get the cached response
        if (cache.TryGetValue(cacheKey, out object? cachedResponse))
        {
            context.Result = new OkObjectResult(cachedResponse);
            return;
        }

        // Execute the action and get the result
        var executedContext = await next();
        var result = executedContext.Result as ObjectResult;

        // Cache the response if successful
        if (result?.Value != null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_timeToLiveSeconds));

            cache.Set(cacheKey, result.Value, cacheEntryOptions);

            // Set HTTP cache headers
            var httpContext = context.HttpContext;
            var cacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue();

            switch (_cacheType)
            {
                case ResponseCacheType.Public:
                    cacheControl.Public = true;
                    cacheControl.Private = false;
                    break;
                case ResponseCacheType.Private:
                    cacheControl.Public = false;
                    cacheControl.Private = true;
                    break;
                case ResponseCacheType.NoStore:
                    cacheControl.NoStore = true;
                    cacheControl.NoCache = true;
                    break;
            }

            cacheControl.MaxAge = TimeSpan.FromSeconds(_timeToLiveSeconds);
            httpContext.Response.GetTypedHeaders().CacheControl = cacheControl;
        }


    }

    private static string GenerateCacheKeyFromRequest(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        
        var keyBuilder = new System.Text.StringBuilder();
        keyBuilder.Append($"{request.Path}");

        // Add query string parameters
        foreach (var (key, value) in request.Query.OrderBy(q => q.Key))
        {
            keyBuilder.Append($"|{key}={value}");
        }

        // Add route values
        foreach (var (key, value) in context.RouteData.Values.OrderBy(r => r.Key))
        {
            keyBuilder.Append($"|{key}={value}");
        }

        // Add action arguments
        foreach (var (key, value) in context.ActionArguments.OrderBy(a => a.Key))
        {
            keyBuilder.Append($"|{key}={JsonSerializer.Serialize(value)}");
        }

        return keyBuilder.ToString();
    }
}

public enum ResponseCacheType
{
    Public,
    Private,
    NoStore
}
