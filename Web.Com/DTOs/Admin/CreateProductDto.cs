using System.ComponentModel.DataAnnotations;

namespace Web.Com.DTOs.Admin;

public class CreateProductDto
{
  [Required]
  [MaxLength(200)]
  public string Name { get; set; } = string.Empty;

  [MaxLength(500)]
  public string ShortDescription { get; set; } = string.Empty;

  [MaxLength(2000)]
  public string Description { get; set; } = string.Empty;

  [Required]
  [Range(0.01, double.MaxValue)]
  public decimal Price { get; set; }

  [Range(0.01, double.MaxValue)]
  public decimal? DiscountPrice { get; set; }

  [Required]
  [Range(0, int.MaxValue)]
  public int Stock { get; set; }

  [MaxLength(50)]
  public string? Weight { get; set; }

  [MaxLength(100)]
  public string? BatchNumber { get; set; }

  [MaxLength(1000)]
  public string? Ingredients { get; set; }

  [Required]
  public Guid CategoryId { get; set; }

  public bool IsMemberOnly { get; set; } = false;
  public bool IsFeatured { get; set; } = false;

  [MaxLength(100)]
  public string? Sku { get; set; }
  public DateTime? ExpiryDate { get; set; }
}

public class UpdateProductDto : CreateProductDto { }
