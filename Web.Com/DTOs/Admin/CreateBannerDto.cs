using System.ComponentModel.DataAnnotations;

namespace Web.Com.DTOs.Admin;

public class CreateBannerDto
{
  [Required]
  [MaxLength(100)]
  public string Title { get; set; } = string.Empty;

  [Required]
  [MaxLength(500)]
  public string Subtitle { get; set; } = string.Empty;

  [Required]
  public string ImageUrl { get; set; } = string.Empty;

  [MaxLength(50)]
  public string Tag { get; set; } = string.Empty;

  public Guid? ProductId { get; set; }
  public Guid? CategoryId { get; set; }
  public string? ExternalUrl { get; set; }

  public bool IsActive { get; set; } = true;
  public bool IsMemberOnly { get; set; } = false;
  public DateTime? EndDate { get; set; }
  public int DisplayOrder { get; set; } = 0;
}

public class UpdateBannerDto : CreateBannerDto { }
