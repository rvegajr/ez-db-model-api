using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Controllers;
using Api.Data;
using Api.Models;
using Xunit;

namespace Test;

public class SampleOrderControllerTests
{
    private readonly SampleDbContext _context;
    private readonly SampleOrderController _controller;

    public SampleOrderControllerTests()
    {
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new SampleDbContext(options);
        _controller = new SampleOrderController(_context);

        // Seed test data
        var product = new SampleProduct
        {
            Id = 1,
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
    public async Task GetOrders_ReturnsAllOrders()
    {
        // Act
        var result = await _controller.GetOrders();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<SampleOrder>>>(result);
        var orders = Assert.IsAssignableFrom<IEnumerable<SampleOrder>>(actionResult.Value);
        Assert.Single(orders);
    }

    [Fact]
    public async Task GetOrder_ReturnsOrder_WhenOrderExists()
    {
        // Act
        var result = await _controller.GetOrder(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrder>>(result);
        var order = Assert.IsType<SampleOrder>(actionResult.Value);
        Assert.Equal(1, order.OrderId);
        Assert.Equal("Test Customer", order.CustomerName);
    }

    [Fact]
    public async Task GetOrder_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Act
        var result = await _controller.GetOrder(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrder>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task CreateOrder_CreatesOrder_WhenModelIsValid()
    {
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
        var result = await _controller.CreateOrder(newOrder);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrder>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var order = Assert.IsType<SampleOrder>(createdAtActionResult.Value);
        Assert.Equal("New Customer", order.CustomerName);
    }

    [Fact]
    public async Task UpdateOrder_UpdatesOrder_WhenOrderExists()
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
        var result = await _controller.UpdateOrder(1, order);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedOrder = await _context.Orders.FindAsync(1);
        Assert.NotNull(updatedOrder);
        Assert.Equal("Updated Customer", updatedOrder.CustomerName);
    }

    [Fact]
    public async Task DeleteOrder_DeletesOrder_WhenOrderExists()
    {
        // Act
        var result = await _controller.DeleteOrder(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var deletedOrder = await _context.Orders.FindAsync(1);
        Assert.Null(deletedOrder);
    }
}
