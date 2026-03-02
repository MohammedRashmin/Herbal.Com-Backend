using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Com.Entities.Identity;

namespace Web.Com.Entities;

public class CartItem
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  public string UserId { get; set; } = string.Empty;

  [ForeignKey("UserId")]
  public ApplicationUser User { get; set; } = null!;

  [Required]
  public Guid ProductId { get; set; }

  [ForeignKey("ProductId")]
  public Product Product { get; set; } = null!;

  [Required]
  [Range(1, int.MaxValue)]
  public int Quantity { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
