using System.Net;
using Newtonsoft.Json;
using Api.Models;

namespace Api.Infrastructure.Middleware;

public class UnauthorizedMiddleware
{
    private readonly RequestDelegate _next;

    public UnauthorizedMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                context.Response.ContentType = "application/json";
            }
            return Task.CompletedTask;
        });

        await _next(context);

        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized && 
            !context.Response.HasStarted)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Unauthorized access"
            };
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
