namespace Test;

public class AdvancedSampleOrderControllerTests : TestBase, IClassFixture<TestWebApplicationFactory<Program>>, IAsyncLifetime
{
    private SampleProduct firstProduct;
    public AdvancedSampleOrderControllerTests(
        TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await _factory.SeedDatabase();
        firstProduct = GetContext().Products.First();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    async Task GetMonthlyStatistics_ReturnsCorrectStats()
    {
        // Arrange
        // Database is seeded in InitializeAsync

        // Act
        var response = await _client.GetAsync("/AdvancedSampleOrder/statistics/monthly");
        var responseContent = await response.Content.ReadAsStringAsync();
        var stats = JsonConvert.DeserializeObject<OrderStatistics>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stats.Should().NotBeNull();
        stats!.TotalOrders.Should().Be(3);
        stats.TotalRevenue.Should().Be(179.94m);
        stats.AverageOrderValue.Should().Be(59.98m);
        stats.UniqueCustomers.Should().Be(2);
    }

    [Fact]
    async Task GetCustomerSummary_ReturnsCorrectSummary()
    {
        // Arrange
        // Database is seeded in InitializeAsync

        // Act
        var response = await _client.GetAsync("/AdvancedSampleOrder/customer/1/summary");
        var responseContent = await response.Content.ReadAsStringAsync();
        var summary = JsonConvert.DeserializeObject<CustomerOrderSummary>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        summary.Should().NotBeNull();
        summary!.CustomerId.Should().Be(1);
        summary.TotalOrders.Should().Be(2);
        summary.TotalSpent.Should().Be(99.96m);
        summary.TopPurchasedProducts.Should().HaveCount(2);
    }

    [Fact]
    async Task Create_ValidatesAndProcessesOrder()
    {
        // Arrange
        // Database is seeded in InitializeAsync

        var order = new SampleOrder
        {
            CustomerId = 3,
            CustomerName = "Test Customer 3",
            OrderDetails = new List<SampleCompoundKeyOrderDetail>
            {
                new() 
                { 
                    ProductId = firstProduct.ProductId, 
                    Quantity = 2
                }
            }
        };

        Console.WriteLine($"Creating order with CustomerId: {order.CustomerId}, CustomerName: {order.CustomerName}");

        // Act
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore
        };
        var json = JsonConvert.SerializeObject(order, settings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/AdvancedSampleOrder", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Request content: {json}");
        Console.WriteLine($"Response status: {response.StatusCode}");
        Console.WriteLine($"Response content: {responseContent}");
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = JsonConvert.DeserializeObject<dynamic>(responseContent);
            Console.WriteLine($"Validation errors: {error}");
        }
        var createdOrder = JsonConvert.DeserializeObject<SampleOrder>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        createdOrder.Should().NotBeNull();
        createdOrder!.OrderId.Should().NotBe(0);
        createdOrder.TotalAmount.Should().Be(firstProduct.Price * 2);
        createdOrder.OrderDate.Date.Should().Be(DateTime.UtcNow.Date);
    }
}
