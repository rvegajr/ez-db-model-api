using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Test.Infrastructure;

namespace Test;

public class HealthControllerTests : IClassFixture<TestWebApplicationFactory<Api.Program>>
{
    private readonly TestWebApplicationFactory<Api.Program> _factory;

    private class TokenResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;
    }

    private class HealthResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    public HealthControllerTests(TestWebApplicationFactory<Api.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsHealthyStatus()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // First, get a token
        var loginModel = new LoginModel
        {
            Username = "admin",
            Password = "admin123"
        };
        var httpResponse = await client.PostAsync("/auth/login",
            new StringContent(JsonConvert.SerializeObject(loginModel), Encoding.UTF8, "application/json"));
        var loginContent = await httpResponse.Content.ReadAsStringAsync();
        TestLogger.LogResponse($"{nameof(Get_ReturnsHealthyStatus)}_Login", httpResponse, loginContent);
        var loginApiResponse = JsonConvert.DeserializeObject<Api.Models.ApiResponse<TokenResponse>>(loginContent, JsonSettings);
        Assert.NotNull(loginApiResponse);
        Assert.True(loginApiResponse.Success);
        var loginResult = loginApiResponse.Data;
        Assert.NotNull(loginResult);

        // Add token to client
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var response = await client.GetAsync("/Health");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        TestLogger.LogResponse(nameof(Get_ReturnsHealthyStatus), response, content);
        Console.WriteLine($"Response content: {content}");
        Console.WriteLine($"Response status code: {response.StatusCode}");
        Console.WriteLine($"Response content type: {response.Content.Headers.ContentType}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResponse = JsonConvert.DeserializeObject<Api.Models.ApiResponse<HealthResponse>>(content, JsonSettings);
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("Healthy", apiResponse.Data.Status);
    }
}
