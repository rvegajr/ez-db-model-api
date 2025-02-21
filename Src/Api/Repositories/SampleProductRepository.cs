using Api.Infrastructure.Base;
using Api.Models;

namespace Api.Repositories;

public interface ISampleProductRepository : IGenericRepository<SampleProduct, int>
{
    Task<IEnumerable<SampleProduct>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
}

public class SampleProductRepository : GenericRepository<SampleProduct, int>, ISampleProductRepository
{
    public SampleProductRepository(SampleDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SampleProduct>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _dbSet
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync();
    }
}
