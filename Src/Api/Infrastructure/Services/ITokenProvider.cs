namespace Api.Infrastructure.Services;

public interface ITokenProvider
{
    string GetJwtToken();
}
