using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Infrastructure.Swagger;

public class ODataDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Remove duplicate operation IDs
        var duplicateOperations = swaggerDoc.Paths
            .SelectMany(p => p.Value.Operations)
            .GroupBy(o => o.Value.OperationId)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var duplicateOperation in duplicateOperations)
        {
            var operations = duplicateOperation.ToList();
            for (int i = 1; i < operations.Count; i++)
            {
                operations[i].Value.OperationId = $"{operations[i].Value.OperationId}_{i}";
            }
        }

        // Add OData specific descriptions
        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                if (path.Key.StartsWith("/odata"))
                {
                    operation.Value.Description += "\n\nThis is an OData endpoint that supports the following query options:\n" +
                        "- $filter: Filter the results using OData syntax\n" +
                        "- $orderby: Order the results using OData syntax\n" +
                        "- $top: Limit the number of results\n" +
                        "- $skip: Skip a number of results\n" +
                        "- $select: Select specific fields\n" +
                        "- $expand: Expand related entities";
                }
            }
        }
    }
}
