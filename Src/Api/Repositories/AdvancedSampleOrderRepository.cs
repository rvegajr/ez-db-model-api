namespace Api.Repositories;

/// <summary>
/// Example implementation of a custom repository that extends GenericRepository
/// with additional business-specific operations
/// </summary>
public class AdvancedSampleOrderRepository : GenericRepository<SampleOrder, int>, IAdvancedSampleOrderRepository
{
    public AdvancedSampleOrderRepository(SampleDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SampleOrder>> GetOrdersByCustomerAsync(string customerName)
    {
        return await _dbSet
            .Where(o => o.CustomerName == customerName)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .ToListAsync();
    }

    public async Task<OrderStatistics> GetMonthlyStatisticsAsync()
    {
        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;
        
        var orders = await _dbSet
            .Where(o => o.OrderDate.Month == currentMonth && o.OrderDate.Year == currentYear)
            .Include(o => o.OrderDetails)
            .ToListAsync();

        return new OrderStatistics
        {
            TotalOrders = orders.Count,
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
            UniqueCustomers = orders.Select(o => o.CustomerName).Distinct().Count()
        };
    }

    public async Task<CustomerOrderSummary> GetCustomerOrderSummaryAsync(int customerId)
    {
        var orders = await _dbSet
            .Where(o => o.CustomerName == customerId.ToString())
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .ToListAsync();

        if (!orders.Any())
            return new CustomerOrderSummary { CustomerId = customerId };

        var topProducts = orders
            .SelectMany(o => o.OrderDetails)
            .GroupBy(od => od.Product.Name)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        return new CustomerOrderSummary
        {
            CustomerId = customerId,
            CustomerName = orders.First().CustomerName,
            TotalOrders = orders.Count,
            TotalSpent = orders.Sum(o => o.TotalAmount),
            LastOrderDate = orders.Max(o => o.OrderDate),
            TopPurchasedProducts = topProducts
        };
    }

    public async Task<bool> ValidateOrderAsync(SampleOrder order)
    {
        // Example validation logic
        if (order.OrderDetails == null || !order.OrderDetails.Any())
            return false;

        foreach (var detail in order.OrderDetails)
        {
            var product = await _context.Set<SampleProduct>()
                .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);
                
            if (product == null || detail.Quantity <= 0)
                return false;
        }

        return true;
    }

    public async Task ProcessNewOrderAsync(SampleOrder order)
    {
        // Example of additional business logic when processing a new order
        order.OrderDate = DateTime.UtcNow;
        order.TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);

        // Example of async operation
        await Task.Delay(100); // Simulating some async work

        // Could add more business logic here:
        // - Update inventory
        // - Create shipping record
        // - Send notifications
        // - Apply discounts
        // etc.
    }
}
