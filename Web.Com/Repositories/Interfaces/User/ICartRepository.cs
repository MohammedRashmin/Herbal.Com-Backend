using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.User;

public interface ICartRepository
{
    Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId);
    Task<CartItem?> GetCartItemAsync(string userId, Guid productId);
    Task AddCartItemAsync(CartItem cartItem);
    Task UpdateCartItemAsync(CartItem cartItem);
    Task RemoveCartItemAsync(Guid cartItemId);
    Task<CartItem?> GetByIdAsync(Guid id);
    Task ClearCartAsync(string userId);
}
