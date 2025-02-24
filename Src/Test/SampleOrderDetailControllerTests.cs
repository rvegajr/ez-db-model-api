using Xunit.Abstractions;

namespace Test;

public class SampleOrderDetailControllerTests : TestBase, IAsyncLifetime
{
    private SampleOrder firstOrder;
    private SampleCompoundKeyOrderDetail firstOrderDetail;
    private readonly ITestOutputHelper _output;

    public async Task InitializeAsync()
    {
        await _factory.SeedDatabase();
        firstOrder = GetContext().Orders.First();
        firstOrderDetail = GetContext().OrderDetails.First();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public SampleOrderDetailControllerTests(
        TestWebApplicationFactory<Program> factory,
        ITestOutputHelper output) : base(factory)
    {
        _output = output;
        TestOutputHelper.Initialize(output);
    }

    [Fact]
    public async Task GetAll_ReturnsOrderDetails_WhenOrderExists()
    {
        // Arrange
        _output.WriteLine("\nTesting: Get All Order Details");

        // Act
        var response = await _client.GetAsync("/SampleCompoundKeyOrderDetail");
        var orderDetails = await response.Content.ReadFromJsonAsync<List<SampleCompoundKeyOrderDetail>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        orderDetails.Should().NotBeNull();
        orderDetails.Should().HaveCount(3);
        orderDetails.Should().Contain(od => od.ProductId == 2);
    }

    [Fact]
    public async Task GetById_ReturnsOrderDetail_WhenOrderDetailExists()
    {
        // Arrange
        _output.WriteLine("\nTesting: Get Order Detail By Compound Key");

        // Act
        // Get first order detail from the list
        var allResponse = await _client.GetAsync("/SampleCompoundKeyOrderDetail");
        var allOrderDetails = await allResponse.Content.ReadFromJsonAsync<List<SampleCompoundKeyOrderDetail>>();
        var firstOrderDetail = allOrderDetails.First();

        var response = await _client.GetAsync($"/SampleCompoundKeyOrderDetail/{firstOrderDetail.OrderId}/{firstOrderDetail.ProductId}");
        var orderDetail = await response.Content.ReadFromJsonAsync<SampleCompoundKeyOrderDetail>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        orderDetail.Should().NotBeNull();
        orderDetail.OrderId.Should().Be(firstOrderDetail.OrderId);
        orderDetail.ProductId.Should().Be(firstOrderDetail.ProductId);
        orderDetail.Quantity.Should().Be(firstOrderDetail.Quantity);
        orderDetail.UnitPrice.Should().Be(firstOrderDetail.UnitPrice);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrderDetailDoesNotExist()
    {
        // Arrange
        _output.WriteLine("\nTesting: Get Non-existent Order Detail");

        // Act
        var response = await _client.GetAsync("/SampleCompoundKeyOrderDetail/999/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_CreatesOrderDetail_WhenModelIsValid()
    {
        // Arrange
        _output.WriteLine("\nTesting: Create New Order Detail");
        // Get first order from the list
        var allResponse = await _client.GetAsync("/SampleOrder");
        var allOrders = await allResponse.Content.ReadFromJsonAsync<List<SampleOrder>>();
        var firstOrder = allOrders.First();

        var newOrderDetail = new SampleCompoundKeyOrderDetail
        {
            OrderId = firstOrder.OrderId,
            ProductId = 3,
            Quantity = 3,
            UnitPrice = 39.99m
        };

        // Act
        var json = JsonConvert.SerializeObject(newOrderDetail);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/SampleCompoundKeyOrderDetail", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Request content: {json}");
        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response content: {responseContent}");
        var createdOrderDetail = JsonConvert.DeserializeObject<SampleCompoundKeyOrderDetail>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        createdOrderDetail.Should().NotBeNull();
        createdOrderDetail.OrderId.Should().Be(firstOrder.OrderId);
        createdOrderDetail.ProductId.Should().Be(3);
        createdOrderDetail.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task Update_UpdatesOrderDetail_WhenOrderDetailExists()
    {
        // Arrange
        var orderDetail = new SampleCompoundKeyOrderDetail
        {
            OrderId = firstOrderDetail.OrderId,
            ProductId = firstOrderDetail.ProductId,
            Quantity = 4,
            UnitPrice = 19.99m
        };

        // Act
        var json = JsonConvert.SerializeObject(orderDetail);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/SampleCompoundKeyOrderDetail/{firstOrderDetail.OrderId}/{firstOrderDetail.ProductId}", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Request content: {json}");
        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response content: {responseContent}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedOrderDetail = await GetContext().OrderDetails.FindAsync(firstOrderDetail.OrderId, firstOrderDetail.ProductId);
        updatedOrderDetail.Should().NotBeNull();
        updatedOrderDetail!.Quantity.Should().Be(4);
    }

    [Fact]
    public async Task Delete_DeletesOrderDetail_WhenOrderDetailExists()
    {
        // Act
        var response = await _client.DeleteAsync($"/SampleCompoundKeyOrderDetail/{firstOrderDetail.OrderId}/{firstOrderDetail.ProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var deletedOrderDetail = await GetContext().OrderDetails.FindAsync(firstOrderDetail.OrderId, firstOrderDetail.ProductId);
        deletedOrderDetail.Should().BeNull();
    }
}
