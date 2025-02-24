using Api.Infrastructure.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Test;

public class TestStartup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public TestStartup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Use the same ApiStartup configuration
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Testing";

        // Configure services using the same setup classes as the main app
        ApiStartup.Instance.ConfigureServices(builder);
        ApiSwaggerSetup.Instance.ConfigureSwagger(builder);

        // Override database with in-memory for testing
        services.AddDbContext<SampleDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Configure OData
        services.AddControllers()
            .AddOData(options => options
                .Select()
                .Filter()
                .OrderBy()
                .SetMaxTop(100)
                .Count()
                .Expand()
                .AddRouteComponents("odata", ApiODataSetup.Instance.GetEdmModel()));

        // Add services from builder to our test services
        foreach (var descriptor in builder.Services)
        {
            services.Add(descriptor);
        }
    }

    public void Configure(IApplicationBuilder app)
    {
        if (_environment.IsDevelopment() || _environment.IsEnvironment("Testing"))
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseODataBatching();
        app.UseODataQueryRequest();
    }
}
