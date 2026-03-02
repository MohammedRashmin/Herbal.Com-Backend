using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.User;

public interface ICartService
{
    Task<IEnumerable<CartItemDto>> GetCartAsync(string userId);
    Task<CartItemDto> AddToCartAsync(string userId, AddToCartDto dto);
    Task RemoveFromCartAsync(Guid cartItemId);
    Task UpdateCartItemAsync(Guid cartItemId, UpdateCartItemDto dto);
    Task ClearCartAsync(string userId);
}
