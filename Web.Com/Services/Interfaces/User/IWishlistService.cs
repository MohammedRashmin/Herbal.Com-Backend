using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.User;

public interface IWishlistService
{
    Task<List<WishlistItemDto>> GetWishlistAsync(string userId);
    Task<bool> AddToWishlistAsync(string userId, int productId);
    Task<bool> RemoveFromWishlistAsync(string userId, int productId);
    Task<bool> IsInWishlistAsync(string userId, int productId);
}
