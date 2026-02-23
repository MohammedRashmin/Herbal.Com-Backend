using Web.Com.DTOs.User;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;
using Web.Com.Repositories.Interfaces.User;
using Web.Com.Services.Interfaces.User;

namespace Web.Com.Services.Implementations.User;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;

    public WishlistService(IWishlistRepository wishlistRepository, IProductRepository productRepository)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
    }

    public async Task<List<WishlistItemDto>> GetWishlistAsync(string userId)
    {
        var items = await _wishlistRepository.GetByUserIdAsync(userId);
        return items.Select(w => new WishlistItemDto
        {
            Id = w.Id,
            ProductId = w.ProductId,
            ProductName = w.Product.Name,
            ProductImageUrl = w.Product.Images.FirstOrDefault()?.ImageUrl,
            Price = w.Product.Price,
            DiscountPrice = w.Product.DiscountPrice,
            Stock = w.Product.Stock,
            AddedAt = w.CreatedAt
        }).ToList();
    }

    public async Task<bool> AddToWishlistAsync(string userId, int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return false;

        var exists = await _wishlistRepository.ExistsAsync(userId, productId);
        if (exists) return false;

        await _wishlistRepository.AddAsync(new WishlistItem { UserId = userId, ProductId = productId });
        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(string userId, int productId)
    {
        var item = await _wishlistRepository.GetAsync(userId, productId);
        if (item == null) return false;

        await _wishlistRepository.DeleteAsync(item);
        return true;
    }

    public async Task<bool> IsInWishlistAsync(string userId, int productId)
    {
        return await _wishlistRepository.ExistsAsync(userId, productId);
    }
}
