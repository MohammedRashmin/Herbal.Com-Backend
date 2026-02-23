using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.User;

public interface ICartRepository
{
    Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId);
    Task<CartItem?> GetCartItemAsync(string userId, int productId);
    Task AddCartItemAsync(CartItem cartItem);
    Task UpdateCartItemAsync(CartItem cartItem);
    Task RemoveCartItemAsync(int cartItemId);
    Task<CartItem?> GetByIdAsync(int id);
    Task ClearCartAsync(string userId);
}
