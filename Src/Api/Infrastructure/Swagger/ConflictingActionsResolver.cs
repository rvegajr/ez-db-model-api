using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData.Routing.Template;

namespace Api.Infrastructure.Swagger;

public class ConflictingActionsResolver
{
    public static string ResolveConflictingActions(ApiDescription description)
    {
        var odataTemplate = description.ActionDescriptor.EndpointMetadata
            .OfType<ODataPathTemplate>()
            .FirstOrDefault();

        if (odataTemplate != null)
        {
            // For OData endpoints, include key parameter in operation ID if present
            var hasKeyParameter = description.ParameterDescriptions.Any(p => p.Name == "key");
            var actionName = description.ActionDescriptor.RouteValues["action"];
            var controllerName = description.ActionDescriptor.RouteValues["controller"];
            
            return hasKeyParameter
                ? $"{controllerName}_{actionName}ById"
                : $"{controllerName}_{actionName}All";
        }

        // For non-OData endpoints, use the default convention
        return description.RelativePath;
    }
}
