using Api.Infrastructure.Base;
using Api.Models;

namespace Api.Repositories;

public interface ISampleCompoundKeyOrderDetailRepository
{
    Task<IEnumerable<SampleCompoundKeyOrderDetail>> GetAllAsync();
    Task<SampleCompoundKeyOrderDetail?> GetByCompoundKeyAsync(int orderId, int productId);
    Task<SampleCompoundKeyOrderDetail> AddAsync(SampleCompoundKeyOrderDetail entity);
    Task<SampleCompoundKeyOrderDetail?> UpdateAsync(SampleCompoundKeyOrderDetail entity);
    Task DeleteAsync(SampleCompoundKeyOrderDetail entity);
    IQueryable<SampleCompoundKeyOrderDetail> GetAsQueryable();
    Task<IEnumerable<SampleCompoundKeyOrderDetail>> GetOrderDetailsByOrderAsync(int orderId);
    Task<decimal> GetOrderDetailTotalAsync(int orderId);
    Task<bool> OrderExistsAsync(int orderId);
    Task<bool> ProductExistsAsync(int productId);
}

public class SampleCompoundKeyOrderDetailRepository : ISampleCompoundKeyOrderDetailRepository
{
    private readonly SampleDbContext _context;

    public SampleCompoundKeyOrderDetailRepository(SampleDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SampleCompoundKeyOrderDetail>> GetAllAsync()
    {
        return await _context.Set<SampleCompoundKeyOrderDetail>()
            .Include(od => od.Product)
            .Include(od => od.Order)
            .ToListAsync();
    }

    public async Task<SampleCompoundKeyOrderDetail?> GetByCompoundKeyAsync(int orderId, int productId)
    {
        return await _context.Set<SampleCompoundKeyOrderDetail>()
            .Include(od => od.Product)
            .Include(od => od.Order)
            .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);
    }

    public async Task<SampleCompoundKeyOrderDetail> AddAsync(SampleCompoundKeyOrderDetail entity)
    {
        Console.WriteLine($"Adding order detail: OrderId={entity.OrderId}, ProductId={entity.ProductId}, Quantity={entity.Quantity}, UnitPrice={entity.UnitPrice}");
        await _context.Set<SampleCompoundKeyOrderDetail>().AddAsync(entity);
        try
        {
            await _context.SaveChangesAsync();
            Console.WriteLine("Successfully added order detail");
            return entity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding order detail: {ex.Message}");
            throw;
        }
    }

    public async Task<SampleCompoundKeyOrderDetail?> UpdateAsync(SampleCompoundKeyOrderDetail entity)
    {
        var existing = await _context.Set<SampleCompoundKeyOrderDetail>()
            .AsTracking()
            .FirstOrDefaultAsync(od => od.OrderId == entity.OrderId && od.ProductId == entity.ProductId);

        if (existing == null)
        {
            return null;
        }

        existing.Quantity = entity.Quantity;
        existing.UnitPrice = entity.UnitPrice;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(SampleCompoundKeyOrderDetail entity)
    {
        _context.Set<SampleCompoundKeyOrderDetail>().Remove(entity);
        await _context.SaveChangesAsync();
    }

    public IQueryable<SampleCompoundKeyOrderDetail> GetAsQueryable()
    {
        return _context.Set<SampleCompoundKeyOrderDetail>()
            .Include(od => od.Product)
            .Include(od => od.Order);
    }

    public async Task<IEnumerable<SampleCompoundKeyOrderDetail>> GetOrderDetailsByOrderAsync(int orderId)
    {
        return await _context.Set<SampleCompoundKeyOrderDetail>()
            .Include(od => od.Product)
            .Where(od => od.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<decimal> GetOrderDetailTotalAsync(int orderId)
    {
        var orderDetail = await _context.Set<SampleCompoundKeyOrderDetail>()
            .Include(od => od.Product)
            .FirstOrDefaultAsync(od => od.OrderId == orderId);

        return orderDetail?.Quantity * orderDetail?.UnitPrice ?? 0;
    }

    public async Task<bool> OrderExistsAsync(int orderId)
    {
        return await _context.Orders.AnyAsync(o => o.OrderId == orderId);
    }

    public async Task<bool> ProductExistsAsync(int productId)
    {
        return await _context.Products.AnyAsync(p => p.ProductId == productId);
    }
}
