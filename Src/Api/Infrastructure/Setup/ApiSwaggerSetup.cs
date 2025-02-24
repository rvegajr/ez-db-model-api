using Microsoft.OpenApi.Models;
using Api.Infrastructure.Swagger;

namespace Api.Infrastructure.Setup;

public class ApiSwaggerSetup
{
    private static ApiSwaggerSetup? _instance;
    private static readonly object _lock = new();

    protected ApiSwaggerSetup() { }

    public static ApiSwaggerSetup Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ApiSwaggerSetup();
                }
            }
            return _instance;
        }
    }

    public virtual void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { 
                Title = "OData API", 
                Version = "v1",
                Description = "API with OData support for querying and filtering data"
            });

            // Add OData query parameters to Swagger
            c.OperationFilter<ODataOperationFilter>();

            // Configure operation IDs to be unique
            c.CustomOperationIds(apiDesc => ConflictingActionsResolver.ResolveConflictingActions(apiDesc));

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Configure XML comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Configure document filter to handle OData endpoints
            c.DocumentFilter<ODataDocumentFilter>();
        });
    }
}
