using Api.Infrastructure.Base;
using Api.Models;

namespace Api.Repositories;

public interface IProductRepository : IGenericRepository<SampleProduct, int>
{
    Task<IEnumerable<SampleProduct>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
}
