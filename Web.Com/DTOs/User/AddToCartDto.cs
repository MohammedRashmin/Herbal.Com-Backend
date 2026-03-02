using System.ComponentModel.DataAnnotations;

namespace Web.Com.DTOs.User;

public class AddToCartDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}
