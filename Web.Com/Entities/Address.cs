using Web.Com.Entities.Identity;

namespace Web.Com.Entities;

public class Address
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string UserId { get; set; } = string.Empty;
  public string Street { get; set; } = string.Empty;
  public string City { get; set; } = string.Empty;
  public string State { get; set; } = string.Empty;
  public string ZipCode { get; set; } = string.Empty;
  public string Country { get; set; } = "USA";
  public bool IsDefault { get; set; } = false;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation properties
  public ApplicationUser User { get; set; } = null!;
}
