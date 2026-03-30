using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Com.Entities;

public class ProductImage
{
  [Key]

  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  public string ImageUrl { get; set; } = string.Empty;

  /// <summary>Cloudinary public ID for deleting the image when product/image is removed.</summary>
  [MaxLength(500)]
  public string? CloudinaryPublicId { get; set; }

  [Required]
  public Guid ProductId { get; set; }

  [ForeignKey("ProductId")]
  public Product? Product { get; set; }
}
