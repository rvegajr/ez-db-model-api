using Api.Data;
using Api.Infrastructure.Base;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class ProductRepository : GenericRepository<SampleProduct, int>, IProductRepository
{
    public ProductRepository(SampleDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SampleProduct>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _dbSet
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync();
    }
}
