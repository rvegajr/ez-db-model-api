using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Api.Models;

namespace Test.Infrastructure;

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
