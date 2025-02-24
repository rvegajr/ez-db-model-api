using System.Text.Json;

namespace Test.Infrastructure;

public static class TestLogger
{
    private static readonly string DebugPath = Path.Combine("/Users/rickyvega/Dev/Noctusoft/ez-db-model-api/debug");

    public static void LogResponse(string testName, HttpResponseMessage response, string content)
    {
        Directory.CreateDirectory(DebugPath);
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{testName}.json";
        var filePath = Path.Combine(DebugPath, fileName);

        var log = new
        {
            TestName = testName,
            StatusCode = response.StatusCode,
            Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value),
            Content = content
        };

        var json = System.Text.Json.JsonSerializer.Serialize(log, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        File.WriteAllText(filePath, json);
    }
}
