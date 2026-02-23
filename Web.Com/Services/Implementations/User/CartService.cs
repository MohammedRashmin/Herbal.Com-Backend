using Web.Com.DTOs.User;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;
using Web.Com.Repositories.Interfaces.User;
using Web.Com.Services.Interfaces.User;

namespace Web.Com.Services.Implementations.User;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<CartItemDto>> GetCartAsync(string userId)
    {
        var items = await _cartRepository.GetCartItemsAsync(userId);
        return items.Select(MapToDto);
    }

    public async Task<CartItemDto> AddToCartAsync(string userId, AddToCartDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null) throw new KeyNotFoundException("Product not found");

        var existingItem = await _cartRepository.GetCartItemAsync(userId, dto.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            await _cartRepository.UpdateCartItemAsync(existingItem);
            return MapToDto(existingItem);
        }

        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            Product = product
        };

        await _cartRepository.AddCartItemAsync(cartItem);
        return MapToDto(cartItem);
    }

    public async Task RemoveFromCartAsync(int cartItemId)
    {
        await _cartRepository.RemoveCartItemAsync(cartItemId);
    }

    public async Task UpdateCartItemAsync(int cartItemId, UpdateCartItemDto dto)
    {
        var cartItem = await _cartRepository.GetByIdAsync(cartItemId);
        if (cartItem == null) throw new KeyNotFoundException("Cart item not found");

        if (dto.Quantity <= 0)
        {
            await _cartRepository.RemoveCartItemAsync(cartItemId);
        }
        else
        {
            if (dto.Quantity > cartItem.Product.Stock)
                throw new InvalidOperationException("Quantity exceeds available stock");

            cartItem.Quantity = dto.Quantity;
            await _cartRepository.UpdateCartItemAsync(cartItem);
        }
    }

    public async Task ClearCartAsync(string userId)
    {
        await _cartRepository.ClearCartAsync(userId);
    }

    private static CartItemDto MapToDto(CartItem item)
    {
        return new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product.Name,
            Price = item.Product.Price,
            DiscountPrice = item.Product.DiscountPrice,
            ProductImageUrl = item.Product.Images.FirstOrDefault()?.ImageUrl,
            Quantity = item.Quantity,
            Stock = item.Product.Stock
        };
    }
}
