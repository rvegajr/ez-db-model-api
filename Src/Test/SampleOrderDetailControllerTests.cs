using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Controllers;
using Api.Data;
using Api.Models;
using Xunit;

namespace Test;

public class SampleOrderDetailControllerTests
{
    private readonly SampleDbContext _context;
    private readonly SampleOrderDetailController _controller;

    public SampleOrderDetailControllerTests()
    {
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new SampleDbContext(options);
        _controller = new SampleOrderDetailController(_context);

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
    public async Task GetOrderDetails_ReturnsOrderDetails_WhenOrderExists()
    {
        // Act
        var result = await _controller.GetOrderDetails(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<SampleOrderDetail>>>(result);
        var orderDetails = Assert.IsAssignableFrom<IEnumerable<SampleOrderDetail>>(actionResult.Value);
        Assert.Single(orderDetails);
    }

    [Fact]
    public async Task GetOrderDetail_ReturnsOrderDetail_WhenOrderDetailExists()
    {
        // Act
        var result = await _controller.GetOrderDetail(1, 1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrderDetail>>(result);
        var orderDetail = Assert.IsType<SampleOrderDetail>(actionResult.Value);
        Assert.Equal(1, orderDetail.OrderId);
        Assert.Equal(1, orderDetail.ProductId);
    }

    [Fact]
    public async Task GetOrderDetail_ReturnsNotFound_WhenOrderDetailDoesNotExist()
    {
        // Act
        var result = await _controller.GetOrderDetail(999, 999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrderDetail>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task CreateOrderDetail_CreatesOrderDetail_WhenModelIsValid()
    {
        // Arrange
        var newOrderDetail = new SampleOrderDetail
        {
            OrderId = 1,
            ProductId = 2,  // Using a different ProductId
            Quantity = 3,
            UnitPrice = 19.99m
        };

        // Act
        var result = await _controller.CreateOrderDetail(newOrderDetail);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleOrderDetail>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var orderDetail = Assert.IsType<SampleOrderDetail>(createdAtActionResult.Value);
        Assert.Equal(3, orderDetail.Quantity);
    }

    [Fact]
    public async Task UpdateOrderDetail_UpdatesOrderDetail_WhenOrderDetailExists()
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
        var result = await _controller.UpdateOrderDetail(1, 1, orderDetail);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedOrderDetail = await _context.OrderDetails.FindAsync(1, 1);
        Assert.NotNull(updatedOrderDetail);
        Assert.Equal(4, updatedOrderDetail.Quantity);
    }

    [Fact]
    public async Task DeleteOrderDetail_DeletesOrderDetail_WhenOrderDetailExists()
    {
        // Act
        var result = await _controller.DeleteOrderDetail(1, 1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var deletedOrderDetail = await _context.OrderDetails.FindAsync(1, 1);
        Assert.Null(deletedOrderDetail);
    }
}
