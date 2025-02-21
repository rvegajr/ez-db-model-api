namespace Api.Interfaces;

public interface ISampleOrder
{
    int OrderId { get; set; }
    DateTime OrderDate { get; set; }
    string CustomerName { get; set; }
    decimal TotalAmount { get; set; }
}
