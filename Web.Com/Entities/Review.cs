using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Com.Entities.Identity;

namespace Web.Com.Entities;

public class Review
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
  [Range(1, 5)]
  public int Rating { get; set; }

  [MaxLength(1000)]
  public string Comment { get; set; } = string.Empty;

  public bool IsApproved { get; set; } = false;
  public bool IsRejected { get; set; } = false;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
