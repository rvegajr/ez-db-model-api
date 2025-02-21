using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Api.Controllers.System;

[ApiController]
[Route("[controller]")]
[Authorize]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ContentResult Get()
    {
        var result = new { Status = "Healthy", Timestamp = DateTime.UtcNow };
        var json = JsonSerializer.Serialize(result);
        return Content(json, "application/json");
    }
}
