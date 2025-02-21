using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net.Http.Headers;
using Api.Models;
using System.Text;

namespace Test;

public class HealthControllerTests
{
    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Get_ReturnsHealthyStatus()
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
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
        Assert.Contains("Timestamp", content);
    }
}
