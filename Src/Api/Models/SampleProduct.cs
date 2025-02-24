namespace Api.Models;

public interface ISampleProduct
{
    int ProductId { get; set; }
    string Name { get; set; }
    decimal Price { get; set; }
    string Description { get; set; }
}

public class SampleProduct : ISampleProduct
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000.00)]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    // Navigation property
    public virtual ICollection<SampleCompoundKeyOrderDetail> OrderDetails { get; set; } = new List<SampleCompoundKeyOrderDetail>();
}
