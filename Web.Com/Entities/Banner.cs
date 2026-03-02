using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Com.Entities;

public class Banner
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  [MaxLength(100)]
  public string Title { get; set; } = string.Empty;

  [Required]
  [MaxLength(200)]
  public string Subtitle { get; set; } = string.Empty;

  [Required]
  public string ImageUrl { get; set; } = string.Empty;

  [MaxLength(50)]
  public string Tag { get; set; } = string.Empty; // e.g., "SPRING SALE", "NEW ARRIVALS"

  // Navigation targets (only one should be set)
  public Guid? ProductId { get; set; }

  [ForeignKey("ProductId")]
  public Product? Product { get; set; }

  public Guid? CategoryId { get; set; }

  [ForeignKey("CategoryId")]
  public Category? Category { get; set; }

  public string? ExternalUrl { get; set; }

  public bool IsMemberOnly { get; set; } = false;

  public DateTime? EndDate { get; set; }

  public int DisplayOrder { get; set; } = 0;

  public bool IsActive { get; set; } = true;
}
