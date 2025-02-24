using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Api.Models;
using Newtonsoft.Json;

namespace Api.Infrastructure.Setup;

public class ApiAuthSetup
{
    private static ApiAuthSetup? _instance;
    private static readonly object _lock = new();

    protected ApiAuthSetup() { }

    public static ApiAuthSetup Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ApiAuthSetup();
                }
            }
            return _instance;
        }
    }

    public virtual void ConfigureAuth(WebApplicationBuilder builder, string issuer, string audience, string key)
    {
        if (!builder.Services.Any(x => x.ServiceType == typeof(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider)))
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        var payload = new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Unauthorized access"
                        };
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(payload));
                    }
                };
            });
        }
    }
}
