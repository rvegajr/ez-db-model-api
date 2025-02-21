using System.ComponentModel.DataAnnotations;
using Api.Interfaces;

namespace Api.Models;

public class SampleProduct : ISampleProduct
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000.00)]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    // Navigation property
    public virtual ICollection<SampleOrderDetail> OrderDetails { get; set; } = new List<SampleOrderDetail>();
}
