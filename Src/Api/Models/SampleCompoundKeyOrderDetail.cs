namespace Api.Models;

// Model with compound key
public class SampleCompoundKeyOrderDetail
{
    [Key]
    [Column(Order = 1)]
    public int OrderId { get; set; }

    [Key]
    [Column(Order = 2)]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    // Navigation properties
    [ForeignKey("OrderId")]
    public virtual SampleOrder? Order { get; set; }

    [ForeignKey("ProductId")]
    public virtual SampleProduct? Product { get; set; }
}
