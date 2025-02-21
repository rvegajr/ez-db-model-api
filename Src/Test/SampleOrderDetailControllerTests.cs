using Xunit.Abstractions;

namespace Test;

public class SampleOrderDetailControllerTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly ISampleOrderDetailRepository _repository;
    private readonly SampleOrderDetailController _controller;

    public SampleOrderDetailControllerTests(ITestOutputHelper output)
    {
        _output = output;
        TestOutputHelper.Initialize(output);
        _repository = new SampleOrderDetailRepository(_context);
        _controller = new SampleOrderDetailController(_repository);

        // Seed test data
        var product = new SampleProduct
        {
            ProductId = 1,
            Name = "Test Product",
            Price = 19.99m,
            Description = "Test Description"
        };
        _context.Products.Add(product);

        var order = new SampleOrder
        {
            OrderId = 1,
            OrderDate = DateTime.UtcNow,
            CustomerName = "Test Customer",
            TotalAmount = 39.98m
        };
        _context.Orders.Add(order);

        var orderDetail = new SampleOrderDetail
        {
            OrderId = 1,
            ProductId = 1,
            Quantity = 2,
            UnitPrice = 19.99m
        };
        _context.OrderDetails.Add(orderDetail);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ReturnsOrderDetails_WhenOrderExists()
    {
        _output.WriteLine("\nTesting: Get All Order Details");
        _output.WriteLine("Checking if we can retrieve all order details from the database");
        // Act
        var result = await _controller.GetAll();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<SampleOrderDetail>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var orderDetails = Assert.IsAssignableFrom<IEnumerable<SampleOrderDetail>>(okResult.Value);
        Assert.Single(orderDetails);
    }

    [Fact]
    public async Task GetById_ReturnsOrderDetail_WhenOrderDetailExists()
    {
        _output.WriteLine("\nTesting: Get Order Detail By ID");
        _output.WriteLine("Checking if we can retrieve a specific order detail using its ID");
        // Act
        var result = await _controller.GetById(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrderDetail>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var orderDetail = Assert.IsType<SampleOrderDetail>(okResult.Value);
        Assert.Equal(1, orderDetail.OrderId);
        Assert.Equal(1, orderDetail.ProductId);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrderDetailDoesNotExist()
    {
        _output.WriteLine("\nTesting: Get Non-existent Order Detail");
        _output.WriteLine("Checking if we get NotFound when requesting an order detail that doesn't exist");
        // Act
        var result = await _controller.GetById(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrderDetail>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task Create_CreatesOrderDetail_WhenModelIsValid()
    {
        _output.WriteLine("\nTesting: Create New Order Detail");
        _output.WriteLine("Checking if we can create a new order detail with valid data");
        // Arrange
        var newOrderDetail = new SampleOrderDetail
        {
            OrderId = 1,
            ProductId = 2,  // Using a different ProductId
            Quantity = 3,
            UnitPrice = 19.99m
        };

        // Act
        var result = await _controller.Create(newOrderDetail);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrderDetail>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var orderDetail = Assert.IsType<SampleOrderDetail>(createdAtActionResult.Value);
        Assert.Equal(3, orderDetail.Quantity);
    }

    [Fact]
    public async Task Update_UpdatesOrderDetail_WhenOrderDetailExists()
    {
        // Arrange
        var orderDetail = new SampleOrderDetail
        {
            OrderId = 1,
            ProductId = 1,
            Quantity = 4,
            UnitPrice = 19.99m
        };

        // Act
        var result = await _controller.Update(1, orderDetail);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedOrderDetail = await _context.OrderDetails.FindAsync(1, 1);
        Assert.NotNull(updatedOrderDetail);
        Assert.Equal(4, updatedOrderDetail.Quantity);
    }

    [Fact]
    public async Task Delete_DeletesOrderDetail_WhenOrderDetailExists()
    {
        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var deletedOrderDetail = await _context.OrderDetails.FindAsync(1, 1);
        Assert.Null(deletedOrderDetail);
    }
}
