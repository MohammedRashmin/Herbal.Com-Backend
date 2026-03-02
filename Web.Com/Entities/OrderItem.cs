using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Com.Entities;

public class OrderItem
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  public Guid OrderId { get; set; }

  [ForeignKey("OrderId")]
  public Order Order { get; set; } = null!;

  [Required]
  public Guid ProductId { get; set; }

  [ForeignKey("ProductId")]
  public Product Product { get; set; } = null!;

  [Required]
  public int Quantity { get; set; }

  [Required]
  [Column(TypeName = "decimal(18,2)")]
  public decimal PriceAtPurchase { get; set; }
}
