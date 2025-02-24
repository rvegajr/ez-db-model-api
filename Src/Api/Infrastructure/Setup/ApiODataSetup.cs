using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Api.Models;

namespace Api.Infrastructure.Setup;

public class ApiODataSetup
{
    private static ApiODataSetup? _instance;
    private static readonly object _lock = new();

    protected ApiODataSetup() { }

    public static ApiODataSetup Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ApiODataSetup();
                }
            }
            return _instance;
        }
    }

    public virtual void ConfigureOData(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers()
            .AddOData(options => options
                .Select()
                .Filter()
                .OrderBy()
                .SetMaxTop(100)
                .Count()
                .Expand()
                .SkipToken()
                .AddRouteComponents("odata", GetEdmModel()));
    }

    public virtual IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();

        // Configure entity sets
        builder.EntitySet<SampleProduct>("SampleProducts");
        builder.EntitySet<SampleOrder>("SampleOrders");
        var orderDetails = builder.EntitySet<SampleCompoundKeyOrderDetail>("SampleCompoundKeyOrderDetails");
        orderDetails.EntityType.HasKey(od => new { od.OrderId, od.ProductId });

        // Configure relationships
        builder.EntityType<SampleOrder>()
            .HasMany(o => o.OrderDetails);

        builder.EntityType<SampleCompoundKeyOrderDetail>()
            .HasOptional(d => d.Order);

        builder.EntityType<SampleCompoundKeyOrderDetail>()
            .HasOptional(d => d.Product);

        return builder.GetEdmModel();
    }
}
