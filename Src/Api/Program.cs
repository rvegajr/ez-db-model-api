using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Api.Data;
using Api.Infrastructure.Services;
using Api.Infrastructure.Builder;
using Api.Infrastructure.Startup;
using Api.Infrastructure.Middleware;
using Api.Repositories;
using Api.Models;
using Newtonsoft.Json;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure services
        builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();
        builder.Services.AddMemoryCache();

        // Configure authentication
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
                        ValidIssuer = "your-test-issuer",
                        ValidAudience = "your-test-audience",
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes("your-test-secret-key-that-is-long-enough-for-hmacsha256"))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            // Skip the default logic
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

        // Add services
        builder.Services.AddDbContext<SampleDbContext>(options =>
            options.UseInMemoryDatabase("SampleDb"));
        builder.Services.AddScoped<ISampleProductRepository, SampleProductRepository>();
        builder.Services.AddScoped<ISampleOrderRepository, SampleOrderRepository>();
        builder.Services.AddScoped<ISampleOrderDetailRepository, SampleOrderDetailRepository>();
        builder.Services.AddScoped<IAuthService>(sp => new AuthService(
            "your-test-secret-key-that-is-long-enough-for-hmacsha256",
            "your-test-issuer",
            "your-test-audience"
        ));

        var app = builder.Build();

        // Configure middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            // Initialize the database
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
                context.Database.EnsureCreated();
            }
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<UnauthorizedMiddleware>();
        app.MapControllers();

        app.Run();
    }
}
