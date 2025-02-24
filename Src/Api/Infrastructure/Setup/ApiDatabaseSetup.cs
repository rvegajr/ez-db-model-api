using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Repositories;
using Api.Controllers.OData;
using Api.Infrastructure.DI;
using Api.Infrastructure.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Infrastructure.Setup;

public class ApiDatabaseSetup
{
    private static ApiDatabaseSetup? _instance;
    private static readonly object _lock = new();

    protected ApiDatabaseSetup() { }

    public static ApiDatabaseSetup Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ApiDatabaseSetup();
                }
            }
            return _instance;
        }
    }

    public virtual void ConfigureDatabase(WebApplicationBuilder builder, string databaseName)
    {
        builder.Services.AddDbContext<SampleDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        // Register generic repository
        builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

        // Auto-register services
        builder.Services.AutoRegisterServices(options =>
        {
            options.AddAssemblyPattern("Api*")
                   .AddAssemblyPattern("Data*");
            options.EnableInterfaceMatching = true;
            options.DefaultLifetime = ServiceLifetime.Scoped;
        });
    }

    public virtual void EnsureDatabaseCreated(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        context.Database.EnsureCreated();
    }
}
