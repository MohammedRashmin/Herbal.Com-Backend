using Web.Com.Entities.Identity;

namespace Web.Com.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "Fixed"
    public decimal DiscountValue { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal MinPurchase { get; set; } = 0;
    public int UsageLimit { get; set; } = 0;
    public int TimesUsed { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
