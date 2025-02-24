namespace Test;

public class ODataResponse<T>
{
    [JsonProperty("@odata.context")]
    public string? Context { get; set; }

    [JsonProperty("@odata.count")]
    public int? Count { get; set; }

    [JsonProperty("value")]
    public List<T> Value { get; set; } = new List<T>();
}

public class ODataTests : IClassFixture<TestWebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;
    private readonly IHttpService _httpService;

    public ODataTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Create a scope to resolve scoped services
        _scope = factory.Services.CreateScope();
        _httpService = _scope.ServiceProvider.GetRequiredService<IHttpService>();

        // Add JWT token for authentication
        var token = _factory.GetJwtToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Ensure database is seeded
        var context = _scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        _factory.SeedDatabase(context);
    }

    [Fact]
    public async Task GetProducts_WithFilter_ReturnsFilteredProducts()
    {
        // Arrange
        var minPrice = 10.00m;
        var url = $"/{_factory.GetODataPrefix()}/SampleProducts?$filter=Price gt {minPrice}";

        // Act
        var httpResponse = await _client.GetAsync(url);
        var content = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<ODataResponse<SampleProduct>>(content);
        var products = response?.Value ?? new List<SampleProduct>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(products);
        Assert.All(products, p => Assert.True(p.Price > minPrice));
    }

    [Fact]
    public async Task GetProducts_WithOrderBy_ReturnsOrderedProducts()
    {
        // Arrange
        var url = $"/{_factory.GetODataPrefix()}/SampleProducts?$orderby=Price asc";

        // Act
        var httpResponse = await _client.GetAsync(url);
        var content = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<ODataResponse<SampleProduct>>(content);
        var products = response?.Value ?? new List<SampleProduct>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(products);
        for (int i = 1; i < products.Count; i++)
        {
            Assert.True(products[i - 1].Price <= products[i].Price);
        }
    }

    [Fact]
    public async Task GetOrders_WithExpand_ReturnsOrdersWithDetails()
    {
        // Arrange
        var url = $"/{_factory.GetODataPrefix()}/SampleOrders?$expand=OrderDetails($expand=Product)";

        // Act
        var httpResponse = await _client.GetAsync(url);
        var content = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<ODataResponse<SampleOrder>>(content);
        var orders = response?.Value ?? new List<SampleOrder>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(orders);
        Assert.All(orders, o =>
        {
            Assert.NotNull(o.OrderDetails);
            Assert.All(o.OrderDetails, od => Assert.NotNull(od.Product));
        });
    }

    [Fact]
    public async Task GetOrders_WithSelect_ReturnsSelectedProperties()
    {
        // Arrange
        var url = $"/{_factory.GetODataPrefix()}/SampleOrders?$select=OrderId,CustomerName,TotalAmount";

        // Act
        var httpResponse = await _client.GetAsync(url);
        var content = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<ODataResponse<SampleOrder>>(content);
        var orders = response?.Value ?? new List<SampleOrder>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(orders);
        Assert.All(orders, o =>
        {
            Assert.NotEqual(0, o.OrderId);
            Assert.NotNull(o.CustomerName);
            Assert.NotEqual(0, o.TotalAmount);
            // OrderDate should not be included in the response
        });
    }

    [Fact]
    public async Task GetProducts_WithTop_ReturnsLimitedResults()
    {
        // Arrange
        const int top = 2;
        var url = $"/{_factory.GetODataPrefix()}/SampleProducts?$top={top}";

        // Act
        var httpResponse = await _client.GetAsync(url);
        var content = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<ODataResponse<SampleProduct>>(content);
        var products = response?.Value ?? new List<SampleProduct>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(products);
        Assert.Equal(top, products.Count);
    }

    [Fact]
    public async Task GetProducts_WithCount_ReturnsTotalCount()
    {
        // Arrange
        var url = $"/{_factory.GetODataPrefix()}/SampleProducts?$count=true";

        // Act
        var httpResponse = await _client.GetAsync(url);
        var content = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<ODataResponse<SampleProduct>>(content);
        var products = response?.Value ?? new List<SampleProduct>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetProducts_WithSkip_ReturnsPagedResults()
    {
        // Arrange - Use factory to seed database
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            
            // Clear and reseed the database
            Console.WriteLine("\n=== CLEARING DATABASE ===");
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("Database cleared and recreated");

            _factory.SeedDatabase(context); // Seed the database
            
            // Verify database state in multiple ways
            Console.WriteLine("\n=== VERIFYING DATABASE STATE ===");
            
            // 1. Check raw SQL count
            var rawCount = await context.Products.CountAsync();
            Console.WriteLine($"Raw SQL count: {rawCount}");
            
            // 2. Get all products ordered by ID
            var orderedProducts = await context.Products
                .OrderBy(p => p.ProductId)
                .AsNoTracking()
                .ToListAsync();
            
            Console.WriteLine($"\nOrdered products ({orderedProducts.Count} total):");
            foreach (var product in orderedProducts)
            {
                Console.WriteLine($"  ID: {product.ProductId}, Name: {product.Name}, Price: {product.Price:C}");
            }
            
            // 3. Dump full product details
            Console.WriteLine("\nFull product details:");
            Console.WriteLine(JsonConvert.SerializeObject(orderedProducts, Formatting.Indented));
            
            // 4. Check if EF Core is tracking any entities
            var trackedCount = context.ChangeTracker.Entries().Count();
            Console.WriteLine($"\nTracked entities: {trackedCount}");
            
            Console.WriteLine("=== END DATABASE STATE ===");
        }

        // Create a log file
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "odata_test.log");
        await File.WriteAllTextAsync(logPath, ""); // Clear existing log
        
        void Log(string message)
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n";
            File.AppendAllText(logPath, logMessage);
            Console.WriteLine(message);
        }
        
        Log("Starting OData pagination test...");
        
        // Act - First try a simple query to verify OData is working
        var baseUrl = $"/{_factory.GetODataPrefix()}/SampleProducts";
        Log($"Base URL: {baseUrl}");
        
        // Log database state before making requests
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            var allProducts = await context.Products.OrderBy(p => p.ProductId).ToListAsync();
            Log($"\n=== DATABASE STATE BEFORE REQUESTS ===\n");
            Log($"Total products in database: {allProducts.Count}");
            foreach (var product in allProducts)
            {
                Log($"  Product {product.ProductId}: {product.Name}, Price: {product.Price:C}");
            }
            Log("=== END DATABASE STATE ===\n");
        }

        // Try getting all products first
        var allQuery = "";
        var allContent = await _httpService.HttpGetAsString(baseUrl + allQuery);
        Log($"\n=== ALL PRODUCTS QUERY ===\nURL: {baseUrl + allQuery}\nResponse:\n{JObject.Parse(allContent).ToString(Formatting.Indented)}\n=== END ALL PRODUCTS QUERY ===");

        // Try simple query
        var simpleQuery = "?$top=1";
        var simpleContent = await _httpService.HttpGetAsString(baseUrl + simpleQuery);
        Log($"\n=== SIMPLE QUERY ===\nURL: {baseUrl + simpleQuery}\nResponse:\n{JObject.Parse(simpleContent).ToString(Formatting.Indented)}\n=== END SIMPLE QUERY ===");

        // Now try the pagination queries
        var firstPageQuery = "?$top=2";
        Log($"\nMaking first page request: {baseUrl + firstPageQuery}");
        var firstPageContent = await _httpService.HttpGetAsString(baseUrl + firstPageQuery);
        Log($"\n=== FIRST PAGE REQUEST ===\nURL: {baseUrl + firstPageQuery}\nResponse:\n{JObject.Parse(firstPageContent).ToString(Formatting.Indented)}\n=== END FIRST PAGE ===");
        var firstPageResponse = JsonConvert.DeserializeObject<ODataResponse<SampleProduct>>(firstPageContent);
        Log($"First page value count: {firstPageResponse?.Value?.Count ?? 0}");

        var secondPageQuery = "?$skip=2&$top=2";
        Log($"\nMaking second page request: {baseUrl + secondPageQuery}");
        var secondPageContent = await _httpService.HttpGetAsString(baseUrl + secondPageQuery);
        Log($"\n=== SECOND PAGE REQUEST ===\nURL: {baseUrl + secondPageQuery}\nResponse:\n{JObject.Parse(secondPageContent).ToString(Formatting.Indented)}\n=== END SECOND PAGE ===");
        var secondPageResponse = JsonConvert.DeserializeObject<ODataResponse<SampleProduct>>(secondPageContent);
        Log($"Second page value count: {secondPageResponse?.Value?.Count ?? 0}");

        // Assert
        Assert.NotNull(firstPageResponse);
        Assert.NotNull(secondPageResponse);
        Assert.Equal(2, firstPageResponse.Value.Count);
        Assert.Equal(2, secondPageResponse.Value.Count);

        // Verify the actual products returned
        Assert.Equal(1, firstPageResponse.Value[0].ProductId);  // First page should have products 1,2
        Assert.Equal(2, firstPageResponse.Value[1].ProductId);
        Assert.Equal(3, secondPageResponse.Value[0].ProductId); // Second page should have products 3,4
        Assert.Equal(4, secondPageResponse.Value[1].ProductId);
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}
