using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.OData.Query;

namespace Api.Infrastructure.Swagger;

public class ODataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<EnableQueryAttribute>().Any())
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$top",
                In = ParameterLocation.Query,
                Description = "The number of records to return",
                Required = false,
                Schema = new OpenApiSchema { Type = "integer" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$skip",
                In = ParameterLocation.Query,
                Description = "The number of records to skip",
                Required = false,
                Schema = new OpenApiSchema { Type = "integer" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$filter",
                In = ParameterLocation.Query,
                Description = "Filter the results using OData syntax",
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$orderby",
                In = ParameterLocation.Query,
                Description = "Order the results using OData syntax",
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$select",
                In = ParameterLocation.Query,
                Description = "Select specific fields using OData syntax",
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "$expand",
                In = ParameterLocation.Query,
                Description = "Expand related entities using OData syntax",
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
