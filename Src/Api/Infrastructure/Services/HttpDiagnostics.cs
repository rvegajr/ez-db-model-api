using System;
using System.Text.Json;
using System.Text;

namespace Api.Infrastructure.Services
{
    public static class HttpDiagnostics 
    {
        private static readonly JsonSerializerOptions _prettyPrintOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string FormatDiagnosticInfo(string methodName, string url, Exception ex, object? payload = null, Type? expectedType = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== HTTP Request Failed in {methodName} ===");
            sb.AppendLine($"URL: {url}");
            sb.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");
            
            if (payload != null)
            {
                sb.AppendLine("\nRequest Payload:");
                try 
                {
                    sb.AppendLine(JsonSerializer.Serialize(payload, _prettyPrintOptions));
                }
                catch (Exception serEx)
                {
                    sb.AppendLine($"[Failed to serialize payload: {serEx.Message}]");
                    sb.AppendLine($"Raw ToString(): {payload}");
                }
            }

            if (expectedType != null)
            {
                sb.AppendLine($"\nExpected Response Type: {expectedType.FullName}");
                sb.AppendLine("Type Properties:");
                foreach (var prop in expectedType.GetProperties())
                {
                    sb.AppendLine($"  - {prop.Name}: {prop.PropertyType.Name}");
                }
            }

            sb.AppendLine("\nException Details:");
            sb.AppendLine($"Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            }
            sb.AppendLine($"Stack Trace:\n{ex.StackTrace}");

            return sb.ToString();
        }

        public static string FormatResponsePayload(object response)
        {
            var sb = new StringBuilder();
            sb.AppendLine("\n=== Response Payload ===");
            try 
            {
                sb.AppendLine(JsonSerializer.Serialize(response, _prettyPrintOptions));
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[Failed to serialize response: {ex.Message}]");
                sb.AppendLine($"Raw ToString(): {response}");
            }
            return sb.ToString();
        }
    }
}
