using System.ComponentModel.DataAnnotations;
using Api.Interfaces;

namespace Api.Models;

public class SampleOrder : ISampleOrder
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    // Navigation property
    public virtual ICollection<SampleOrderDetail> OrderDetails { get; set; } = new List<SampleOrderDetail>();
}
