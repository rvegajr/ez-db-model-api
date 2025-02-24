using Api.Infrastructure.Services;
using Api.Infrastructure.Setup;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure API infrastructure
        const string issuer = "your-test-issuer";
        const string audience = "your-test-audience";
        const string key = "your-test-secret-key-that-is-long-enough-for-hmacsha256";

        // Configure base services
        ApiStartup.Instance.ConfigureServices(builder);

        // Configure authentication
        ApiAuthSetup.Instance.ConfigureAuth(builder, issuer, audience, key);

        // Configure Swagger
        ApiSwaggerSetup.Instance.ConfigureSwagger(builder);

        // Configure database
        ApiDatabaseSetup.Instance.ConfigureDatabase(builder, "SampleDb");

        // Add auth service
        builder.Services.AddScoped<IAuthService>(sp => new AuthService(key, issuer, audience));

        var app = builder.Build();

        // Configure middleware
        ApiStartup.Instance.ConfigureMiddleware(app);

        if (app.Environment.IsDevelopment())
        {
            ApiDatabaseSetup.Instance.EnsureDatabaseCreated(app);
        }

        app.Run();
    }
}
