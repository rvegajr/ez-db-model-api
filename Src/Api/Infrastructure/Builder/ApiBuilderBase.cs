using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Api.Infrastructure.Builder;

public abstract class ApiBuilderBase
{
    private static ApiBuilderBase? _instance;
    protected WebApplicationBuilder _builder;
    protected IServiceCollection _services => _builder.Services;

    protected ApiBuilderBase(string[] args)
    {
        _builder = WebApplication.CreateBuilder(args);
    }

    public static T GetInstance<T>(string[] args) where T : ApiBuilderBase
    {
        if (_instance == null)
        {
            _instance = (T)Activator.CreateInstance(typeof(T), new object[] { args })!;
        }
        return (T)_instance;
    }

    public virtual ApiBuilderBase ConfigureServices()
    {
        // Add base services
        _services.AddMemoryCache();
        _services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });
        _services.AddEndpointsApiExplorer();
        
        return this;
    }

    public virtual ApiBuilderBase ConfigureAuthentication(string jwtKey, string issuer, string audience)
    {
        _services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        return this;
    }

    public virtual ApiBuilderBase ConfigureSwagger()
    {
        _services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

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
        });

        return this;
    }

    public virtual WebApplication Build()
    {
        return _builder.Build();
    }
}
