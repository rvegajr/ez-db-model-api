namespace Api.Models;

public class CustomerOrderSummary
{
    public int CustomerId { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime LastOrderDate { get; set; }
    public List<string> TopPurchasedProducts { get; set; } = new();
}
