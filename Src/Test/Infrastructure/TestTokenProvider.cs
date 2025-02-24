using Api.Infrastructure.Services;

namespace Test.Infrastructure;

public class TestTokenProvider<TProgram> : ITokenProvider where TProgram : class
{
    private readonly TestWebApplicationFactory<TProgram> _factory;

    public TestTokenProvider(TestWebApplicationFactory<TProgram> factory)
    {
        _factory = factory;
    }

    public string GetJwtToken()
    {
        return _factory.GetJwtToken();
    }
}
