using System.ComponentModel.DataAnnotations;

namespace Web.Com.Entities;

public class Category
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  [MaxLength(100)]
  public string Name { get; set; } = string.Empty;

  [MaxLength(500)]
  public string Description { get; set; } = string.Empty;

  public string? ImageUrl { get; set; }

  /// <summary>Cloudinary public ID for deleting the image when category is removed.</summary>
  [MaxLength(500)]
  public string? CloudinaryPublicId { get; set; }

  public int DisplayOrder { get; set; } = 0;

  public bool IsActive { get; set; } = true;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Relationship: A category can have multiple products
  public ICollection<Product> Products { get; set; } = new List<Product>();
}
