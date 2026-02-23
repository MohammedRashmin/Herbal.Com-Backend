using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.User;

public interface ICartService
{
    Task<IEnumerable<CartItemDto>> GetCartAsync(string userId);
    Task<CartItemDto> AddToCartAsync(string userId, AddToCartDto dto);
    Task RemoveFromCartAsync(int cartItemId);
    Task UpdateCartItemAsync(int cartItemId, UpdateCartItemDto dto);
    Task ClearCartAsync(string userId);
}
