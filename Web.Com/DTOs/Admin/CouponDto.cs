using System.ComponentModel.DataAnnotations;

namespace Web.Com.DTOs.Admin;

public class CreateCouponDto
{
  [Required]
  [StringLength(50)]
  public string Code { get; set; } = string.Empty;

  [Required]
  public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "Fixed"

  [Required]
  [Range(0.01, double.MaxValue)]
  public decimal DiscountValue { get; set; }

  [Required]
  public DateTime ExpiryDate { get; set; }

  [Range(0, double.MaxValue)]
  public decimal MinPurchase { get; set; } = 0;

  [Range(0, int.MaxValue)]
  public int UsageLimit { get; set; } = 0; // 0 = unlimited

  public bool IsActive { get; set; } = true;
}

public class UpdateCouponDto
{
  [Required]
  public string DiscountType { get; set; } = "Percentage";

  [Required]
  [Range(0.01, double.MaxValue)]
  public decimal DiscountValue { get; set; }

  [Required]
  public DateTime ExpiryDate { get; set; }

  [Range(0, double.MaxValue)]
  public decimal MinPurchase { get; set; } = 0;

  [Range(0, int.MaxValue)]
  public int UsageLimit { get; set; } = 0;

  public bool IsActive { get; set; } = true;
}

public class CouponResponseDto
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Code { get; set; } = string.Empty;
  public string DiscountType { get; set; } = string.Empty;
  public decimal DiscountValue { get; set; }
  public DateTime ExpiryDate { get; set; }
  public decimal MinPurchase { get; set; }
  public int UsageLimit { get; set; }
  public int TimesUsed { get; set; }
  public bool IsActive { get; set; }
  public DateTime CreatedAt { get; set; }
  public bool IsExpired => DateTime.UtcNow > ExpiryDate;
  public bool IsUsageLimitReached => UsageLimit > 0 && TimesUsed >= UsageLimit;
}

public class ValidateCouponResponseDto
{
  public bool IsValid { get; set; }
  public string Message { get; set; } = string.Empty;
  public string DiscountType { get; set; } = string.Empty;
  public decimal DiscountValue { get; set; }
}
