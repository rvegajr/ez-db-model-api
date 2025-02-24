using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Repositories;

namespace Api.Infrastructure.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string databaseName)
    {
        services.AddDbContext<SampleDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        services.AddScoped<ISampleProductRepository, SampleProductRepository>();
        services.AddScoped<ISampleOrderRepository, SampleOrderRepository>();
        services.AddScoped<ISampleCompoundKeyOrderDetailRepository, SampleCompoundKeyOrderDetailRepository>();

        return services;
    }

    public static void EnsureDatabaseCreated(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        context.Database.EnsureCreated();
    }
}
