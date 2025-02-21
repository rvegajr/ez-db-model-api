using Api.Infrastructure.Base;
using Api.Models;

namespace Api.Repositories;

public interface ISampleOrderDetailRepository : IGenericRepository<SampleOrderDetail, int>
{
    Task<IEnumerable<SampleOrderDetail>> GetOrderDetailsByOrderAsync(int orderId);
    Task<decimal> GetOrderDetailTotalAsync(int orderDetailId);
}

public class SampleOrderDetailRepository : GenericRepository<SampleOrderDetail, int>, ISampleOrderDetailRepository
{
    public SampleOrderDetailRepository(SampleDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SampleOrderDetail>> GetOrderDetailsByOrderAsync(int orderId)
    {
        return await _dbSet
            .Include(od => od.Product)
            .Where(od => od.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<decimal> GetOrderDetailTotalAsync(int orderId)
    {
        var orderDetail = await _dbSet
            .Include(od => od.Product)
            .FirstOrDefaultAsync(od => od.OrderId == orderId);

        return orderDetail?.Quantity * orderDetail?.UnitPrice ?? 0;
    }

    public override async Task<SampleOrderDetail?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(od => od.Product)
            .FirstOrDefaultAsync(od => od.OrderId == id);
    }
}
