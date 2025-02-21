using Api.Infrastructure.Base;
using Api.Models;

namespace Api.Repositories;

public interface IOrderDetailRepository : IGenericRepository<SampleOrderDetail, int>
{
    Task<IEnumerable<SampleOrderDetail>> GetOrderDetailsByOrderAsync(int orderId);
    Task<decimal> GetOrderDetailTotalAsync(int orderDetailId);
}
