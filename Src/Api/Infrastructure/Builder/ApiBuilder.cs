using Api.Data;
using Api.Infrastructure.Services;
using Api.Repositories;

namespace Api.Infrastructure.Builder;

public class ApiBuilder : ApiBuilderBase
{
    public ApiBuilder(string[] args) : base(args)
    {
    }

    public static ApiBuilder Create(string[] args)
    {
        return GetInstance<ApiBuilder>(args);
    }

    public override ApiBuilderBase ConfigureServices()
    {
        base.ConfigureServices();

        // Configure URLs
        _builder.WebHost.UseUrls("http://localhost:5001");

        // Add DbContext
        _services.AddDbContext<SampleDbContext>(options =>
            options.UseInMemoryDatabase("SampleDb"));

        // Register repositories
        _services.AddScoped<ISampleProductRepository, SampleProductRepository>();
        _services.AddScoped<ISampleOrderRepository, SampleOrderRepository>();
        _services.AddScoped<ISampleCompoundKeyOrderDetailRepository, SampleCompoundKeyOrderDetailRepository>();

        // Add health checks
        _services.AddHealthChecks();

        return this;
    }

    public ApiBuilder ConfigureAuth()
    {
        var key = "your-super-secret-key-with-at-least-32-characters";
        var issuer = "your-issuer";
        var audience = "your-audience";

        _services.Configure<AuthSettings>(settings =>
        {
            settings.Key = key;
            settings.Issuer = issuer;
            settings.Audience = audience;
        });

        _services.AddScoped<IAuthService, AuthService>();
        base.ConfigureAuthentication(key, issuer, audience);

        return this;
    }
}
