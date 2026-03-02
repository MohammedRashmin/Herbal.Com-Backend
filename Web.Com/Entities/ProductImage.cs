using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Com.Entities;

public class ProductImage
{
  [Key]

  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  public string ImageUrl { get; set; } = string.Empty;

  [Required]
  public Guid ProductId { get; set; }

  [ForeignKey("ProductId")]
  public Product? Product { get; set; }
}
