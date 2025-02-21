using Api.Data;
using Api.Infrastructure.Base;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class OrderRepository : GenericRepository<SampleOrder, int>, IOrderRepository
{
    public OrderRepository(SampleDbContext context) : base(context)
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
