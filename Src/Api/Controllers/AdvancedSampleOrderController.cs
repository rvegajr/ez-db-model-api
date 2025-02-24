using Api.Models;
using Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AdvancedSampleOrderController : ControllerBase
{
    private readonly SampleDbContext _context;

    public AdvancedSampleOrderController(SampleDbContext context)
    {
        _context = context;
    }

    [HttpGet("statistics/monthly")]
    public async Task<ActionResult<OrderStatistics>> GetMonthlyStatistics()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
            .ToListAsync();

        var stats = new OrderStatistics
        {
            TotalOrders = orders.Count,
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
            UniqueCustomers = orders.Select(o => o.CustomerName).Distinct().Count()
        };

        return Ok(stats);
    }

    [HttpGet("customer/{customerId}/summary")]
    public async Task<ActionResult<CustomerOrderSummary>> GetCustomerSummary(int customerId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();

        if (!orders.Any())
            return NotFound();

        var summary = new CustomerOrderSummary
        {
            CustomerId = customerId,
            TotalOrders = orders.Count,
            TotalSpent = orders.Sum(o => o.TotalAmount),
            CustomerName = orders.FirstOrDefault()?.CustomerName ?? string.Empty,
            LastOrderDate = orders.Max(o => o.OrderDate),
            TopPurchasedProducts = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => od.Product.Name)
                .OrderByDescending(g => g.Sum(od => od.Quantity))
                .Take(5)
                .Select(g => g.Key)
                .ToList()
        };

        return Ok(summary);
    }

    [HttpPost]
    public async Task<ActionResult<SampleOrder>> Create(SampleOrder order)
    {
        try
        {
            if (order == null)
            {
                return BadRequest(new { Error = "Order cannot be null" });
            }

            if (order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return BadRequest(new { Error = "At least one order detail is required" });
            }

            // Validate that all products exist and set order details
            foreach (var orderDetail in order.OrderDetails)
            {
                var product = await _context.Products.FindAsync(orderDetail.ProductId);
                if (product == null)
                {
                    return BadRequest(new { Error = $"Product {orderDetail.ProductId} not found" });
                }

                orderDetail.UnitPrice = product.Price;
                orderDetail.Order = order;
            }

            order.OrderDate = DateTime.UtcNow;
            order.TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Create), new { id = order.OrderId }, order);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating order: {ex}");
            return StatusCode(500, new { Error = "An error occurred while processing your request." });
        }
    }
}
