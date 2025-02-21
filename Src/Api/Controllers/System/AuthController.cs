using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Api.Models;
using Api.Services;

namespace Api.Controllers.System;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ContentResult Login([FromBody] LoginModel model)
    {
        if (!_authService.ValidateCredentials(model.Username, model.Password))
        {
            Response.StatusCode = 401;
            return Content("", "application/json");
        }

        var token = _authService.GenerateJwtToken(model.Username);
        var result = JsonSerializer.Serialize(new { Token = token });
        return Content(result, "application/json");
    }
}
