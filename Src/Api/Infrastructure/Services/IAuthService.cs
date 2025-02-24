using Api.Models;

namespace Api.Infrastructure.Services;

public interface IAuthService
{
    bool ValidateCredentials(LoginModel credentials);
    string GenerateToken(string username);
}
