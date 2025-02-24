using Xunit.Abstractions;

namespace Test;

public class SampleOrderControllerTests : TestBase, IAsyncLifetime
{
    private SampleOrder firstOrder;
    private readonly ITestOutputHelper _output;
    public SampleOrderControllerTests(
        TestWebApplicationFactory<Program> factory,
        ITestOutputHelper output) : base(factory)
    {
        _output = output;
        TestOutputHelper.Initialize(output);
    }

    public async Task InitializeAsync()
    {
        await _factory.SeedDatabase();
        firstOrder = GetContext().Orders.First();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAll_ReturnsAllOrders()
    {
        _output.WriteLine("\nTesting: Get All Orders");
        _output.WriteLine("Checking if we can retrieve all orders from the database");
        // Act
        var response = await _client.GetAsync("/SampleOrder");
        var responseContent = await response.Content.ReadAsStringAsync();
        var orders = JsonConvert.DeserializeObject<IEnumerable<SampleOrder>>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        orders.Should().NotBeNull();
        orders.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetById_ReturnsOrder_WhenOrderExists()
    {
        _output.WriteLine("\nTesting: Get Order By ID");
        _output.WriteLine("Checking if we can retrieve a specific order using its ID");
        // Act
        // Get first order from the list
        var allResponse = await _client.GetAsync("/SampleOrder");
        var allOrders = await allResponse.Content.ReadFromJsonAsync<List<SampleOrder>>();
        var firstOrder = allOrders.First();

        var response = await _client.GetAsync($"/SampleOrder/{firstOrder.OrderId}");
        var responseContent = await response.Content.ReadAsStringAsync();
        var order = JsonConvert.DeserializeObject<SampleOrder>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        order.Should().NotBeNull();
        order!.OrderId.Should().Be(firstOrder.OrderId);
        order.CustomerName.Should().Be("Test Customer");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        _output.WriteLine("\nTesting: Get Non-existent Order");
        _output.WriteLine("Checking if we get NotFound when requesting an order that doesn't exist");
        // Act
        var response = await _client.GetAsync("/SampleOrder/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_CreatesOrder_WhenModelIsValid()
    {
        _output.WriteLine("\nTesting: Create New Order");
        _output.WriteLine("Checking if we can create a new order with valid data");
        // Arrange
        var newOrder = new SampleOrder
        {
            OrderDate = DateTime.UtcNow,
            CustomerName = "New Customer",
            OrderDetails = new List<SampleCompoundKeyOrderDetail>
            {
                new SampleCompoundKeyOrderDetail
                {
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 19.99m
                }
            }
        };

        // Act
        var json = JsonConvert.SerializeObject(newOrder);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/SampleOrder", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var order = JsonConvert.DeserializeObject<SampleOrder>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        order.Should().NotBeNull();
        order!.CustomerName.Should().Be("New Customer");
    }

    [Fact]
    public async Task Update_UpdatesOrder_WhenOrderExists()
    {
        // Arrange
        var order = new SampleOrder
        {
            OrderId = 1,
            OrderDate = DateTime.UtcNow,
            CustomerName = "Updated Customer",
            TotalAmount = 39.98m,
            OrderDetails = new List<SampleCompoundKeyOrderDetail>
            {
                new SampleCompoundKeyOrderDetail
                {
                    OrderId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 19.99m
                }
            }
        };

        // Act
        var json = JsonConvert.SerializeObject(order);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/SampleOrder/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedOrder = await GetContext().Orders.FindAsync(1);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.CustomerName.Should().Be("Updated Customer");
    }

    [Fact]
    public async Task Delete_DeletesOrder_WhenOrderExists()
    {
        // Act
        // Get first order from the list
        var allResponse = await _client.GetAsync("/SampleOrder");
        var allOrders = await allResponse.Content.ReadFromJsonAsync<List<SampleOrder>>();
        var firstOrder = allOrders.First();

        var response = await _client.DeleteAsync($"/SampleOrder/{firstOrder.OrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var deletedOrder = await GetContext().Orders.FindAsync(1);
        deletedOrder.Should().BeNull();
    }
}
