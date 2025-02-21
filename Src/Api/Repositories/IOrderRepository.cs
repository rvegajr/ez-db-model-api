using Api.Infrastructure.Base;
using Api.Models;

namespace Api.Repositories;

public interface IOrderRepository : IGenericRepository<SampleOrder, int>
{
    Task<IEnumerable<SampleOrder>> GetOrdersByCustomerAsync(string customerName);
    Task<decimal> GetTotalOrderValueAsync(int orderId);
}
