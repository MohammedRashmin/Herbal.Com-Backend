using Web.Com.DTOs.User;

namespace Web.Com.Services.User;

public interface ICartService
{
    Task<IEnumerable<CartItemDto>> GetCartAsync(string userId);
    Task<CartItemDto> AddToCartAsync(string userId, AddToCartDto dto);
    Task RemoveFromCartAsync(int cartItemId);
    Task ClearCartAsync(string userId);
}
