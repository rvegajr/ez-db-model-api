using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Test.Infrastructure;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private string _odataPrefix;


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
    public void SeedDatabase(SampleDbContext context)
    {
        try
        {
            Console.WriteLine("Starting database seeding...");
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            Console.WriteLine("Database recreated");

            Console.WriteLine("\n=== SEEDING DATABASE ===");
            Console.WriteLine($"Database exists: {context.Database.EnsureCreated()}");
            Console.WriteLine($"Products before seeding: {context.Products.Count()}");

            // Clear existing products
            var existingProducts = context.Products.ToList();
            if (existingProducts.Any())
            {
                Console.WriteLine($"Removing {existingProducts.Count} existing products...");
                context.Products.RemoveRange(existingProducts);
                context.SaveChanges();
            }

            // Create new test products
            var products = new List<SampleProduct>();
            for (int i = 1; i <= 5; i++)
            {
                var product = new SampleProduct
                {
                    ProductId = i,
                    Name = $"Sample Product {i}",
                    Price = i * 10.00m,
                    Description = $"This is sample product {i}"
                };
                products.Add(product);
                Console.WriteLine($"Created product: {JsonConvert.SerializeObject(product)}");
            }

            Console.WriteLine($"Adding {products.Count} products...");
            context.Products.AddRange(products);
            context.SaveChanges();
            Console.WriteLine($"Products after seeding: {context.Products.Count()}");
            Console.WriteLine("=== END SEEDING DATABASE ===");
            
            var seededProducts = context.Products.OrderBy(p => p.ProductId).ToList();
            Console.WriteLine($"Products after seeding: {context.Products.Count()}");
            Console.WriteLine($"Seeded products: {JsonConvert.SerializeObject(seededProducts)}");

            // Verify seeding
            var verifyProducts = context.Products.OrderBy(p => p.ProductId).ToList();
            Console.WriteLine($"Verified products count: {verifyProducts.Count}");
            foreach (var product in verifyProducts)
            {
                Console.WriteLine($"Verified product: {JsonConvert.SerializeObject(product)}");
            }

            // Add test orders if they don't exist
            if (!context.Orders.Any())
            {
                var orders = new List<SampleOrder>();
                for (int i = 1; i <= 2; i++)
                {
                    var order = new SampleOrder
                    {
                        OrderId = i,
                        CustomerName = $"Test Customer {i}",
                        OrderDate = DateTime.UtcNow.AddDays(-i),
                        TotalAmount = i * 50.99m
                    };
                    orders.Add(order);
                }

                Console.WriteLine($"Adding {orders.Count} orders...");
                context.Orders.AddRange(orders);
                context.SaveChanges();
                Console.WriteLine($"Orders after seeding: {context.Orders.Count()}");

                // Add order details
                var orderDetails = new List<SampleOrderDetail>
                {
                    new SampleOrderDetail { OrderId = orders[0].OrderId, ProductId = seededProducts[0].ProductId, Quantity = 2, UnitPrice = 10.99m },
                    new SampleOrderDetail { OrderId = orders[0].OrderId, ProductId = seededProducts[1].ProductId, Quantity = 1, UnitPrice = 30.99m },
                    new SampleOrderDetail { OrderId = orders[1].OrderId, ProductId = seededProducts[1].ProductId, Quantity = 4, UnitPrice = 20.99m }
                };

                Console.WriteLine($"Adding {orderDetails.Count} order details...");
                context.OrderDetails.AddRange(orderDetails);
                context.SaveChanges();
            }

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var finalProducts = context.Products.ToList();
            Console.WriteLine($"Final products in database: {JsonConvert.SerializeObject(finalProducts, jsonSettings)}");
            Console.WriteLine($"Database seeded with {context.Products.Count()} products, {context.Orders.Count()} orders, and {context.OrderDetails.Count()} order details.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding database: {ex.Message}");
            throw;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing registrations
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SampleDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var authDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider));
            if (authDescriptor != null)
            {
                services.Remove(authDescriptor);
            }

            var authServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthService));
            if (authServiceDescriptor != null)
            {
                services.Remove(authServiceDescriptor);
            }



            // Remove existing registrations
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<SampleDbContext>) ||
                d.ServiceType == typeof(SampleDbContext) ||
                d.ServiceType == typeof(ISampleProductRepository) ||
                d.ServiceType == typeof(ISampleOrderRepository) ||
                d.ServiceType == typeof(ISampleOrderDetailRepository)).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Remove OData registrations
            var odataDescriptors = services.Where(d =>
                d.ServiceType.Name.Contains("OData") ||
                d.ServiceType.Name.Contains("Edm") ||
                d.ImplementationType?.Name.Contains("OData") == true ||
                d.ImplementationType?.Name.Contains("Edm") == true).ToList();

            foreach (var descriptor in odataDescriptors)
            {
                services.Remove(descriptor);
            }

            // Add test database
            var databaseName = "TestDb_" + Guid.NewGuid().ToString();
            services.AddDbContext<SampleDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
                options.EnableSensitiveDataLogging();
            });

            // Add repositories
            services.AddScoped<ISampleProductRepository, SampleProductRepository>();
            services.AddScoped<ISampleOrderRepository, SampleOrderRepository>();
            services.AddScoped<ISampleOrderDetailRepository, SampleOrderDetailRepository>();

            // Configure MVC and OData
            var edmModel = Api.Infrastructure.Setup.EdmModelBuilder.GetEdmModel();
            _odataPrefix = $"odata-{Guid.NewGuid()}";
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                })
                .AddOData(options =>
                {
                    options.AddRouteComponents(_odataPrefix, edmModel);
                    options.Select().Filter().OrderBy().SetMaxTop(100).Count().Expand();
                    options.EnableQueryFeatures(maxTopValue: 100);
                    options.EnableNoDollarQueryOptions = true;
                });

            // Configure API behavior
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
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

            // Add seed data
            var sp = services.BuildServiceProvider();
            try
            {
                using (var scope = sp.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
                    SeedDatabase(context);
                    Console.WriteLine($"Successfully seeded database {databaseName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database {databaseName}: {ex}");
                throw;
            }

            // Add required services
            var jwtKey = "your-test-secret-key-that-is-long-enough-for-hmacsha256-with-extra-padding-123456789";
            var issuer = "your-test-issuer";
            var audience = "your-test-audience";

            services.AddScoped<IAuthService>(sp => new AuthService(
                jwtKey,
                issuer,
                audience
            ));

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

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        });
    }
}
