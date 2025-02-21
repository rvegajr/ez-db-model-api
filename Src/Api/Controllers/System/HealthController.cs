using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Api.Models;

namespace Api.Controllers.System;

[ApiController]
[Route("[controller]")]
[Authorize]
public class HealthController : BaseController
{
    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            var result = new HealthResponse
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            };
            Response.ContentType = "application/json";
            return Ok(result, "System is healthy");
        }
        catch (Exception ex)
        {
            Response.ContentType = "application/json";
            return HandleException(ex);
        }
    }
}
