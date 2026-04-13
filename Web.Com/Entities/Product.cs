using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Com.Entities;

public class Product
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  [MaxLength(200)]
  public string Name { get; set; } = string.Empty;

  [MaxLength(500)]
  public string ShortDescription { get; set; } = string.Empty;

  [MaxLength(2000)]
  public string Description { get; set; } = string.Empty;

  [Required]
  [Column(TypeName = "decimal(18,2)")]
  public decimal Price { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? DiscountPrice { get; set; }

  [Required]
  public int Stock { get; set; }

  [MaxLength(50)]
  public string? Weight { get; set; }

  [MaxLength(100)]
  public string? BatchNumber { get; set; }

  [MaxLength(1000)]
  public string? Ingredients { get; set; }

  [Column(TypeName = "decimal(3,2)")]
  public decimal AverageRating { get; set; } = 0;

  [Required]
  public Guid CategoryId { get; set; }

  [ForeignKey("CategoryId")]
  public Category Category { get; set; } = null!;

  [MaxLength(100)]
  public string? Sku { get; set; }

  public DateTime? ExpiryDate { get; set; }

  public bool IsMemberOnly { get; set; } = false;

  public bool IsFeatured { get; set; } = false;

  [MaxLength(100)]
  public string? ServingSize { get; set; }

  public int? ServingsPerContainer { get; set; }

  [MaxLength(500)]
  public string? Badges { get; set; }  // JSON array

  [MaxLength(2000)]
  public string? Benefits { get; set; }  // JSON array

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Relationships
  public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
  public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
