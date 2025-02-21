using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Api.Models;
using Api.Infrastructure.Services;

namespace Api.Controllers.System;

[ApiController]
[Route("[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginModel model)
    {
        try
        {
            if (model == null)
            {
                Response.ContentType = "application/json";
                return BadRequest("Login model cannot be null");
            }

            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                Response.ContentType = "application/json";
                return BadRequest("Username and password are required");
            }

            if (!_authService.ValidateCredentials(model))
            {
                Response.ContentType = "application/json";
                return Unauthorized("Invalid username or password");
            }

            var token = _authService.GenerateToken(model.Username);
            Response.ContentType = "application/json";
            return Ok(new TokenResponse { Token = token }, "Login successful");
        }
        catch (Exception ex)
        {
            Response.ContentType = "application/json";
            return HandleException(ex);
        }
    }
}
