using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace Api.Infrastructure.Services
{
    public class HttpService : IHttpService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly HttpClient _client;

        public HttpService(HttpClient client) 
        {
            _client = client;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        #region http

        public async Task<T> HttpGet<T>(string url) where T : new()
        {
            try
            {
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response received from {url}:\n{content}");
                
                try 
                {
                    return JsonSerializer.Deserialize<T>(content, _jsonOptions);
                }
                catch (JsonException jsonEx)
                {
                    var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                        nameof(HttpGet), 
                        url, 
                        jsonEx,
                        expectedType: typeof(T)
                    );
                    throw new Exception($"Deserialization failed. {diagnostics}", jsonEx);
                }
            }
            catch (Exception ex)
            {
                var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                    nameof(HttpGet), 
                    url, 
                    ex,
                    expectedType: typeof(T)
                );
                throw new Exception($"{nameof(HttpService)}.{nameof(HttpGet)} failed. {diagnostics}", ex);
            }
        }

        public async Task<bool> HttpPut<T>(string url, T putPayload)
        {
            try
            {
                // Use the injected client
                var json = JsonSerializer.Serialize(putPayload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"Sending PUT request to {url} with payload:\n{json}");
                
                var response = await _client.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response received:\n{responseContent}");
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                    nameof(HttpPut), 
                    url, 
                    ex,
                    putPayload
                );
                throw new Exception($"{nameof(HttpService)}.{nameof(HttpPut)} failed. {diagnostics}", ex);
            }
        }

        public async Task<bool> HttpPost<T>(string url, T postPayload)
        {
            try
            {
                // Use the injected client
                var json = JsonSerializer.Serialize(postPayload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"Sending POST request to {url} with payload:\n{json}");
                
                var response = await _client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response received:\n{responseContent}");
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                    nameof(HttpPost), 
                    url, 
                    ex,
                    postPayload
                );
                throw new Exception($"{nameof(HttpService)}.{nameof(HttpPost)} failed. {diagnostics}", ex);
            }
        }

        public async Task<T> HttpPostReturn<T>(string url, T postPayload)
        {
            try
            {
                // Use the injected client
                var json = JsonSerializer.Serialize(postPayload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"Sending POST request to {url} with payload:\n{json}");
                
                var response = await _client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response received:\n{responseContent}");
                
                try 
                {
                    var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                    Console.WriteLine(HttpDiagnostics.FormatResponsePayload(result));
                    return result;
                }
                catch (JsonException jsonEx)
                {
                    var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                        nameof(HttpPostReturn), 
                        url, 
                        jsonEx,
                        postPayload,
                        typeof(T)
                    );
                    throw new Exception($"Deserialization failed. {diagnostics}", jsonEx);
                }
            }
            catch (Exception ex)
            {
                var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                    nameof(HttpPostReturn), 
                    url, 
                    ex,
                    postPayload,
                    typeof(T)
                );
                throw new Exception($"{nameof(HttpService)}.{nameof(HttpPostReturn)} failed. {diagnostics}", ex);
            }
        }

        public async Task<bool> HttpDelete(string url)
        {
            try
            {
                Console.WriteLine($"Sending DELETE request to {url}");
                
                var response = await _client.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response received:\n{responseContent}");
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                    nameof(HttpDelete), 
                    url, 
                    ex
                );
                throw new Exception($"{nameof(HttpService)}.{nameof(HttpDelete)} failed. {diagnostics}", ex);
            }
        }

        public async Task<T> HttpGetAsString<T>(string url) where T : new()
        {
            try
            {
                string responseResult = await this.HttpGetAsString(url);
                Console.WriteLine($"Response received from {url}:\n{responseResult}");
                
                try 
                {
                    var result = JsonSerializer.Deserialize<T>(responseResult, _jsonOptions);
                    Console.WriteLine(HttpDiagnostics.FormatResponsePayload(result));
                    return result;
                }
                catch (JsonException jsonEx)
                {
                    var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                        nameof(HttpGetAsString), 
                        url, 
                        jsonEx,
                        expectedType: typeof(T)
                    );
                    throw new Exception($"Deserialization failed. {diagnostics}", jsonEx);
                }
            }
            catch (Exception ex)
            {
                var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                    nameof(HttpGetAsString), 
                    url, 
                    ex,
                    expectedType: typeof(T)
                );
                throw new Exception($"{nameof(HttpService)}.{nameof(HttpGetAsString)} failed. {diagnostics}", ex);
            }
        }

        public async Task<string> HttpGetAsString(string url)
        {
            try
            {
                Console.WriteLine($"Sending GET request to {url}");
                
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response received:\n{content}");
                
                return content;
            }
            catch (Exception ex)
            {
                var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                    nameof(HttpGetAsString), 
                    url, 
                    ex
                );
                throw new Exception($"{nameof(HttpService)}.{nameof(HttpGetAsString)} failed. {diagnostics}", ex);
            }
        }

        public async Task<byte[]> HttpGetAsBinary(string url)
        {
            try
            {
                Console.WriteLine($"Sending GET request to {url} for binary data");
                
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsByteArrayAsync();
                Console.WriteLine($"Received {content.Length} bytes from {url}");
                
                return content;
            }
            catch (Exception ex)
            {
                var diagnostics = HttpDiagnostics.FormatDiagnosticInfo(
                    nameof(HttpGetAsBinary), 
                    url, 
                    ex
                );
                throw new Exception($"{nameof(HttpService)}.{nameof(HttpGetAsBinary)} failed. {diagnostics}", ex);
            }
        }

        #endregion http
    }
}
