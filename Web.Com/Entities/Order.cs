using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Com.Entities.Identity;

namespace Web.Com.Entities;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [MaxLength(20)]
    public string PaymentMethod { get; set; } = string.Empty; // "PayPal", "Card", "COD"

    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending"; // "Pending", "Paid", "Failed"

    [MaxLength(100)]
    public string? PayPalOrderId { get; set; }

    [MaxLength(100)]
    public string? TrackingNumber { get; set; }

    [Required]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    // Relationship
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
