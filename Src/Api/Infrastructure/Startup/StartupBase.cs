using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Startup;

public abstract class StartupBase
{
    private static StartupBase? _instance;
    protected readonly WebApplication _app;

    protected StartupBase(WebApplication app)
    {
        _app = app;
    }

    public static T GetInstance<T>(WebApplication app) where T : StartupBase
    {
        if (_instance == null)
        {
            _instance = (T)Activator.CreateInstance(typeof(T), app)!;
        }
        return (T)_instance;
    }

    public virtual StartupBase Configure()
    {
        if (_app.Environment.IsDevelopment())
        {
            ConfigureDevelopment();
        }

        ConfigureMiddleware();
        ConfigureEndpoints();

        return this;
    }

    protected virtual void ConfigureDevelopment()
    {
        _app.UseSwagger();
        _app.UseSwaggerUI();

        // Initialize the database with sample data
        using (var scope = _app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            context.Database.EnsureCreated();
        }
    }

    protected virtual void ConfigureMiddleware()
    {
        _app.UseRouting();
        _app.UseAuthentication();
        _app.UseAuthorization();
    }

    protected virtual void ConfigureEndpoints()
    {
        _app.MapControllers();
        _app.MapHealthChecks("/health");
    }

    public virtual void Run()
    {
        _app.Run();
    }
}
