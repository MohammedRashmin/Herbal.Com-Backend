namespace Web.Com.DTOs.User;

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
}

public class UpdateCartItemDto
{
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}
