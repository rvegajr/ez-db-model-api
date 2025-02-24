using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Test.Infrastructure;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private string _odataPrefix = "/odata";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing registrations
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<SampleDbContext>) ||
                d.ServiceType == typeof(SampleDbContext) ||
                d.ServiceType == typeof(ISampleProductRepository) ||
                d.ServiceType == typeof(ISampleOrderRepository) ||
                d.ServiceType == typeof(ISampleCompoundKeyOrderDetailRepository) ||
                d.ServiceType == typeof(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider) ||
                d.ServiceType == typeof(IAuthService)).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add test database
            var databaseName = "TestDb_" + Guid.NewGuid().ToString();
            services.AddDbContext<SampleDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.EnableSensitiveDataLogging();
            });

            // Add repositories
            services.AddScoped<ISampleProductRepository, SampleProductRepository>();
            services.AddScoped<ISampleOrderRepository, SampleOrderRepository>();
            services.AddScoped<ISampleCompoundKeyOrderDetailRepository, SampleCompoundKeyOrderDetailRepository>();

            // Configure auth
            var jwtKey = "your-test-secret-key-that-is-long-enough-for-hmacsha256";
            var issuer = "test-issuer";
            var audience = "test-audience";

            services.Configure<AuthSettings>(options =>
            {
                options.Key = jwtKey;
                options.Issuer = issuer;
                options.Audience = audience;
            });
            services.AddScoped<IAuthService, AuthService>();

            // Configure MVC and OData
            var edmModel = Api.Infrastructure.Setup.EdmModelBuilder.GetEdmModel();
            _odataPrefix = $"odata-{Guid.NewGuid()}";
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                })
                .AddOData(options =>
                {
                    options.AddRouteComponents(_odataPrefix, edmModel);
                    options.Select().Filter().OrderBy().SetMaxTop(100).Count().Expand();
                    options.EnableQueryFeatures(maxTopValue: 100);
                    options.EnableNoDollarQueryOptions = true;
                });



            // Add common services
            services.AddEndpointsApiExplorer();
            services.AddHealthChecks();
            services.AddMemoryCache();
            
            // Register token provider
            services.AddScoped<ITokenProvider>(sp => new TestTokenProvider<TProgram>(this));
            
            // Register HttpService with test server client
            services.AddScoped<IHttpService>(sp => {
                var client = CreateClient();
                var token = GetJwtToken();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return new HttpService(client);
            });

            // Add logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddDebug();
            });

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add JWT authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
        });
    }

    public string GetJwtToken()
    {
        using var scope = Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        return authService.GenerateToken("test-user");
    }

    public string GetODataPrefix()
    {
        return _odataPrefix;
    }

    public async Task SeedDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

        try
        {
            Console.WriteLine("Starting database seeding...");
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            Console.WriteLine("Database recreated");

            // Clear existing data
            context.OrderDetails.RemoveRange(context.OrderDetails);
            context.Orders.RemoveRange(context.Orders);
            context.Products.RemoveRange(context.Products);
            context.SaveChanges();

            // Add test products with fixed IDs
            var products = new List<SampleProduct>();
            var product1 = new SampleProduct
            {
                ProductId = 1,
                Name = "Test Product 1",
                Price = 19.99m,
                Description = "Test Description 1"
            };
            context.Products.Add(product1);
            await context.SaveChangesAsync();
            products.Add(product1);

            var product2 = new SampleProduct
            {
                ProductId = 2,
                Name = "Test Product 2",
                Price = 29.99m,
                Description = "Test Description 2"
            };
            context.Products.Add(product2);
            await context.SaveChangesAsync();
            products.Add(product2);

            var product3 = new SampleProduct
            {
                ProductId = 3,
                Name = "Test Product 3",
                Price = 39.99m,
                Description = "Test Description 3"
            };
            context.Products.Add(product3);
            await context.SaveChangesAsync();
            products.Add(product3);

            Console.WriteLine($"Products created with IDs: {product1.ProductId}, {product2.ProductId}, {product3.ProductId}");

            // Add test orders with correct IDs
            var order1 = new SampleOrder
            {
                OrderId = 1,
                CustomerId = 1,
                OrderDate = DateTime.UtcNow.AddDays(-1),
                CustomerName = "Test Customer",
                TotalAmount = 39.98m
            };
            context.Orders.Add(order1);
            await context.SaveChangesAsync();

            var order2 = new SampleOrder
            {
                OrderId = 2,
                CustomerId = 1,
                OrderDate = DateTime.UtcNow,
                CustomerName = "Test Customer",
                TotalAmount = 59.98m
            };
            context.Orders.Add(order2);
            await context.SaveChangesAsync();

            var order3 = new SampleOrder
            {
                OrderId = 3,
                CustomerId = 2,
                OrderDate = DateTime.UtcNow,
                CustomerName = "Test Customer 2",
                TotalAmount = 79.98m
            };
            context.Orders.Add(order3);
            await context.SaveChangesAsync();

            // Add order details
            var orderDetail1 = new SampleCompoundKeyOrderDetail
            {
                OrderId = order1.OrderId,
                ProductId = products[0].ProductId,
                Quantity = 2,
                UnitPrice = 19.99m,
                Order = order1,
                Product = products[0]
            };
            context.OrderDetails.Add(orderDetail1);
            await context.SaveChangesAsync();

            var orderDetail2 = new SampleCompoundKeyOrderDetail
            {
                OrderId = order2.OrderId,
                ProductId = products[1].ProductId,
                Quantity = 2,
                UnitPrice = 29.99m,
                Order = order2,
                Product = products[1]
            };
            context.OrderDetails.Add(orderDetail2);
            await context.SaveChangesAsync();

            var orderDetail3 = new SampleCompoundKeyOrderDetail
            {
                OrderId = order3.OrderId,
                ProductId = products[2].ProductId,
                Quantity = 2,
                UnitPrice = 39.99m,
                Order = order3,
                Product = products[2]
            };
            context.OrderDetails.Add(orderDetail3);
            await context.SaveChangesAsync();

            Console.WriteLine("Database seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding database: {ex.Message}");
            throw;
        }
    }
}
