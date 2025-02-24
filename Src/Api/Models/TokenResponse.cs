using System.Text.Json.Serialization;

namespace Api.Models;

public class TokenResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
