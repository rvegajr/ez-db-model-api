using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public interface IHttpService
    {
        Task<T> HttpGet<T>(string url) where T : new();
        Task<bool> HttpPut<T>(string url, T putPayload);
        Task<bool> HttpPost<T>(string url, T postPayload);
        Task<T> HttpPostReturn<T>(string url, T postPayload);
        Task<bool> HttpDelete(string url);
        Task<T> HttpGetAsString<T>(string url) where T : new();
        Task<string> HttpGetAsString(string url);
        Task<byte[]> HttpGetAsBinary(string url);
    }
}
