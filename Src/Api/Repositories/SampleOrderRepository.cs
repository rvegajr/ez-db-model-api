using Api.Infrastructure.Base;
using Api.Models;

namespace Api.Repositories;

public interface ISampleOrderRepository : IGenericRepository<SampleOrder, int>
{
    Task<IEnumerable<SampleOrder>> GetOrdersByCustomerAsync(string customerName);
    Task<decimal> GetTotalOrderValueAsync(int orderId);
}

public class SampleOrderRepository : GenericRepository<SampleOrder, int>, ISampleOrderRepository
{
    public SampleOrderRepository(SampleDbContext context) : base(context)
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

    public async Task<decimal> GetTotalOrderValueAsync(int orderId)
    {
        var order = await _dbSet
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        return order?.TotalAmount ?? 0;
    }

    public override async Task<SampleOrder?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }
}
