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
        builder.EntitySet<SampleCompoundKeyOrderDetail>("SampleCompoundKeyOrderDetails");

        // Configure relationships
        builder.EntityType<SampleOrder>()
            .HasMany(o => o.OrderDetails);

        builder.EntityType<SampleCompoundKeyOrderDetail>()
            .HasOptional(d => d.Order);

        builder.EntityType<SampleCompoundKeyOrderDetail>()
            .HasOptional(d => d.Product);

        // Configure compound key
        builder.EntityType<SampleCompoundKeyOrderDetail>()
            .HasKey(d => new { d.OrderId, d.ProductId });

        return builder.GetEdmModel();
    }
}
