using Api.Infrastructure.Base;
using Api.Models;

namespace Api.Repositories;

/// <summary>
/// Example of a custom repository interface that extends IGenericRepository
/// with additional business-specific operations
/// </summary>
public interface IAdvancedSampleOrderRepository : IGenericRepository<SampleOrder, int>
{
    #region Query Methods
    /// <summary>
    /// Retrieves all orders for a specific customer
    /// </summary>
    /// <param name="customerName">Name of the customer</param>
    /// <returns>Collection of orders for the customer</returns>
    Task<IEnumerable<SampleOrder>> GetOrdersByCustomerAsync(string customerName);

    /// <summary>
    /// Gets order statistics for the current month
    /// </summary>
    /// <returns>Statistics including total orders, revenue, and customer counts</returns>
    Task<OrderStatistics> GetMonthlyStatisticsAsync();

    /// <summary>
    /// Gets a detailed order summary for a specific customer
    /// </summary>
    /// <param name="customerId">ID of the customer</param>
    /// <returns>Summary of customer's order history</returns>
    Task<CustomerOrderSummary> GetCustomerOrderSummaryAsync(int customerId);
    #endregion

    #region Order Processing
    /// <summary>
    /// Validates an order before processing
    /// </summary>
    /// <param name="order">Order to validate</param>
    /// <returns>True if order is valid, false otherwise</returns>
    Task<bool> ValidateOrderAsync(SampleOrder order);

    /// <summary>
    /// Processes a new order with additional business logic
    /// </summary>
    /// <param name="order">Order to process</param>
    Task ProcessNewOrderAsync(SampleOrder order);
    #endregion
}

public class OrderStatistics
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int UniqueCustomers { get; set; }
}

public class CustomerOrderSummary
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastOrderDate { get; set; }
    public List<string> TopPurchasedProducts { get; set; } = new();
}
