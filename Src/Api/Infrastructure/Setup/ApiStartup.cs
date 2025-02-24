using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Api.Models;

namespace Api.Infrastructure.Setup;

public static class EdmModelBuilder
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();

        // Configure entity sets
        builder.EntitySet<SampleProduct>("SampleProducts");
        builder.EntitySet<SampleOrder>("SampleOrders");
        builder.EntitySet<SampleOrderDetail>("SampleOrderDetails");

        // Configure relationships
        builder.EntityType<SampleOrder>()
            .HasMany(o => o.OrderDetails);

        builder.EntityType<SampleOrderDetail>()
            .HasOptional(d => d.Order);

        builder.EntityType<SampleOrderDetail>()
            .HasOptional(d => d.Product);

        return builder.GetEdmModel();
    }
}

public class ApiStartup
{
    private static ApiStartup? _instance;
    private static readonly object _lock = new();

    protected ApiStartup() { }

    public static ApiStartup Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ApiStartup();
                }
            }
            return _instance;
        }
    }

    public virtual void ConfigureServices(WebApplicationBuilder builder)
    {
        // Configure JSON options
        builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            })
            .AddOData(options => options
                .Select()
                .Filter()
                .OrderBy()
                .SetMaxTop(100)
                .Count()
                .Expand()
                .AddRouteComponents("odata", EdmModelBuilder.GetEdmModel()));

        // Add common services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHealthChecks();
        builder.Services.AddMemoryCache();

        // Configure API behavior
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    public virtual void ConfigureMiddleware(WebApplication app)
    {
        app.UseODataRouteDebug();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.UseODataBatching();
    }
}
