using Api.Controllers.Entity;
using Api.Infrastructure.Base;
using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Test;

public class AdvancedSampleOrderControllerTests : TestBase
{
    private readonly AdvancedSampleOrderController _controller;
    private readonly IAdvancedSampleOrderRepository _repository;

    public AdvancedSampleOrderControllerTests()
    {
        _repository = new AdvancedSampleOrderRepository(_context);
        _controller = new AdvancedSampleOrderController(_repository);
    }

    [Fact]
    public async Task GetMonthlyStatistics_ReturnsCorrectStats()
    {
        // Arrange
        var orders = new List<SampleOrder>
        {
            new()
            {
                OrderId = 1,
                CustomerName = "Customer 1",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 100.00m,
                OrderDetails = new List<SampleOrderDetail>
                {
                    new() { OrderId = 1, ProductId = 1, Quantity = 2, UnitPrice = 50.00m }
                }
            },
            new()
            {
                OrderId = 2,
                CustomerName = "Customer 2",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 150.00m,
                OrderDetails = new List<SampleOrderDetail>
                {
                    new() { OrderId = 2, ProductId = 1, Quantity = 3, UnitPrice = 50.00m }
                }
            }
        };
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetMonthlyStatistics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stats = Assert.IsType<OrderStatistics>(okResult.Value);
        Assert.Equal(2, stats.TotalOrders);
        Assert.Equal(250.00m, stats.TotalRevenue);
        Assert.Equal(125.00m, stats.AverageOrderValue);
        Assert.Equal(2, stats.UniqueCustomers);
    }

    [Fact]
    public async Task GetCustomerSummary_ReturnsCorrectSummary()
    {
        // Arrange
        var products = new List<SampleProduct>
        {
            new() { ProductId = 1, Name = "Product 1", Price = 50.00m },
            new() { ProductId = 2, Name = "Product 2", Price = 75.00m }
        };
        await _context.Products.AddRangeAsync(products);

        var orders = new List<SampleOrder>
        {
            new()
            {
                OrderId = 1,
                CustomerName = "1", // Using ID as name for test
                OrderDate = DateTime.UtcNow.AddDays(-1),
                TotalAmount = 100.00m,
                OrderDetails = new List<SampleOrderDetail>
                {
                    new() { OrderId = 1, ProductId = 1, Quantity = 2, UnitPrice = 50.00m }
                }
            },
            new()
            {
                OrderId = 2,
                CustomerName = "1", // Same customer
                OrderDate = DateTime.UtcNow,
                TotalAmount = 150.00m,
                OrderDetails = new List<SampleOrderDetail>
                {
                    new() { OrderId = 2, ProductId = 2, Quantity = 2, UnitPrice = 75.00m }
                }
            }
        };
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCustomerSummary(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var summary = Assert.IsType<CustomerOrderSummary>(okResult.Value);
        Assert.Equal(1, summary.CustomerId);
        Assert.Equal(2, summary.TotalOrders);
        Assert.Equal(250.00m, summary.TotalSpent);
        Assert.Equal(2, summary.TopPurchasedProducts.Count);
    }

    [Fact]
    public async Task Create_ValidatesAndProcessesOrder()
    {
        // Arrange
        var product = new SampleProduct { ProductId = 1, Name = "Test Product", Price = 50.00m };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var order = new SampleOrder
        {
            CustomerName = "Test Customer",
            OrderDetails = new List<SampleOrderDetail>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 50.00m }
            }
        };

        // Act
        var result = await _controller.Create(order);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdOrder = Assert.IsType<SampleOrder>(createdResult.Value);
        Assert.NotEqual(0, createdOrder.OrderId);
        Assert.Equal(100.00m, createdOrder.TotalAmount);
        Assert.Equal(DateTime.UtcNow.Date, createdOrder.OrderDate.Date);
    }
}
