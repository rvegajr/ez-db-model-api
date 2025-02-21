namespace Api.Infrastructure.Startup;

public class ApiStartup : StartupBase
{
    public ApiStartup(WebApplication app) : base(app)
    {
    }

    public static ApiStartup Create(WebApplication app)
    {
        return GetInstance<ApiStartup>(app);
    }

    protected override void ConfigureDevelopment()
    {
        base.ConfigureDevelopment();

        // Add any API-specific development configuration here
    }

    protected override void ConfigureMiddleware()
    {
        base.ConfigureMiddleware();

        // Add any API-specific middleware configuration here
    }

    protected override void ConfigureEndpoints()
    {
        base.ConfigureEndpoints();

        // Add any API-specific endpoint configuration here
    }
}
