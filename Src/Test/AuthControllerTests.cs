using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Test;

public class AuthControllerTests
{
    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        await using var application = new WebApplicationFactory<Api.Program>();
        using var client = application.CreateClient();
        var loginModel = new LoginModel
        {
            Username = "admin",
            Password = "admin123"
        };

        // Act
        var response = await client.PostAsync("/auth/login",
            new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TokenResponse>(content);
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        await using var application = new WebApplicationFactory<Api.Program>();
        using var client = application.CreateClient();
        var loginModel = new LoginModel
        {
            Username = "invalid",
            Password = "invalid"
        };

        // Act
        var response = await client.PostAsync("/auth/login",
            new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        await using var application = new WebApplicationFactory<Api.Program>();
        using var client = application.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_WithValidToken_ReturnsOk()
    {
        // Arrange
        await using var application = new WebApplicationFactory<Api.Program>();
        using var client = application.CreateClient();

        // First, get a token
        var loginModel = new LoginModel
        {
            Username = "admin",
            Password = "admin123"
        };
        var loginResponse = await client.PostAsync("/auth/login",
            new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json"));
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<TokenResponse>(loginContent);
        Assert.NotNull(loginResult);

        // Add token to client
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }
}
