using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Repositories;

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

        // Add repositories
        builder.Services.AddScoped<ISampleProductRepository, SampleProductRepository>();
        builder.Services.AddScoped<ISampleOrderRepository, SampleOrderRepository>();
        builder.Services.AddScoped<ISampleOrderDetailRepository, SampleOrderDetailRepository>();
    }

    public virtual void EnsureDatabaseCreated(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        context.Database.EnsureCreated();
    }
}
