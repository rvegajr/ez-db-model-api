using Xunit.Abstractions;

namespace Test;

public class SampleOrderControllerTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly ISampleOrderRepository _repository;
    private readonly SampleOrderController _controller;

    public SampleOrderControllerTests(ITestOutputHelper output)
    {
        _output = output;
        TestOutputHelper.Initialize(output);
        _repository = new SampleOrderRepository(_context);
        _controller = new SampleOrderController(_repository);

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
            TotalAmount = 39.98m,
            OrderDetails = new List<SampleOrderDetail>
            {
                new SampleOrderDetail
                {
                    OrderId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 19.99m
                }
            }
        };
        _context.Orders.Add(order);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ReturnsAllOrders()
    {
        _output.WriteLine("\nTesting: Get All Orders");
        _output.WriteLine("Checking if we can retrieve all orders from the database");
        // Act
        var result = await _controller.GetAll();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<SampleOrder>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var orders = Assert.IsAssignableFrom<IEnumerable<SampleOrder>>(okResult.Value);
        Assert.Single(orders);
    }

    [Fact]
    public async Task GetById_ReturnsOrder_WhenOrderExists()
    {
        _output.WriteLine("\nTesting: Get Order By ID");
        _output.WriteLine("Checking if we can retrieve a specific order using its ID");
        // Act
        var result = await _controller.GetById(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrder>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var order = Assert.IsType<SampleOrder>(okResult.Value);
        Assert.Equal(1, order.OrderId);
        Assert.Equal("Test Customer", order.CustomerName);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        _output.WriteLine("\nTesting: Get Non-existent Order");
        _output.WriteLine("Checking if we get NotFound when requesting an order that doesn't exist");
        // Act
        var result = await _controller.GetById(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrder>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
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
            OrderDetails = new List<SampleOrderDetail>
            {
                new SampleOrderDetail
                {
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 19.99m
                }
            }
        };

        // Act
        var result = await _controller.Create(newOrder);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrder>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var order = Assert.IsType<SampleOrder>(createdAtActionResult.Value);
        Assert.Equal("New Customer", order.CustomerName);
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
            OrderDetails = new List<SampleOrderDetail>
            {
                new SampleOrderDetail
                {
                    OrderId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 19.99m
                }
            }
        };

        // Act
        var result = await _controller.Update(1, order);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedOrder = await _context.Orders.FindAsync(1);
        Assert.NotNull(updatedOrder);
        Assert.Equal("Updated Customer", updatedOrder.CustomerName);
    }

    [Fact]
    public async Task Delete_DeletesOrder_WhenOrderExists()
    {
        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var deletedOrder = await _context.Orders.FindAsync(1);
        Assert.Null(deletedOrder);
    }
}
