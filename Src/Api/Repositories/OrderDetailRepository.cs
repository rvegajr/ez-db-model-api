using Api.Data;
using Api.Infrastructure.Base;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class OrderDetailRepository : GenericRepository<SampleOrderDetail, int>, IOrderDetailRepository
{
    public OrderDetailRepository(SampleDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SampleOrderDetail>> GetOrderDetailsByOrderAsync(int orderId)
    {
        return await _dbSet
            .Include(od => od.Product)
            .Where(od => od.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<decimal> GetOrderDetailTotalAsync(int orderDetailId)
    {
        var orderDetail = await _dbSet
            .Include(od => od.Product)
            .FirstOrDefaultAsync(od => od.OrderDetailId == orderDetailId);

        return orderDetail?.Quantity * orderDetail?.UnitPrice ?? 0;
    }

    public override async Task<SampleOrderDetail?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(od => od.Product)
            .FirstOrDefaultAsync(od => od.OrderDetailId == id);
    }
}
