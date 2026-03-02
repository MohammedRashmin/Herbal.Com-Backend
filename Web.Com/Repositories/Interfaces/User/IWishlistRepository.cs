using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.User;

public interface IWishlistRepository
{
    Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId);
    Task<WishlistItem?> GetAsync(string userId, Guid productId);
    Task AddAsync(WishlistItem item);
    Task DeleteAsync(WishlistItem item);
    Task<bool> ExistsAsync(string userId, Guid productId);
}
