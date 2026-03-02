using Web.Com.Entities.Identity;

namespace Web.Com.Entities;

public class WishlistItem
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string UserId { get; set; } = string.Empty;
  public Guid ProductId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation properties
  public ApplicationUser User { get; set; } = null!;
  public Product Product { get; set; } = null!;
}
