using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Newtonsoft.Json;

namespace Api.Controllers;

public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleException(Exception ex)
    {
        // Log the exception here
        var response = ApiResponse<object>.Error("An unexpected error occurred.");
        return new ContentResult
        {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }

    protected IActionResult Ok<T>(T data, string message = "Operation completed successfully")
    {
        var response = ApiResponse<T>.Ok(data, message);
        return new ContentResult
        {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    protected IActionResult BadRequest(string message)
    {
        var response = ApiResponse<object>.Error(message);
        return new ContentResult
        {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    protected IActionResult Unauthorized(string message = "Unauthorized access")
    {
        var response = ApiResponse<object>.Error(message);
        Response.Headers.Add("WWW-Authenticate", "Bearer");
        return new ContentResult
        {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status401Unauthorized
        };
    }

    protected IActionResult NotFound(string message = "Resource not found")
    {
        var response = ApiResponse<object>.Error(message);
        return new ContentResult
        {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status404NotFound
        };
    }
}
