using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.Admin;

public interface IProductService
{
    // Admin
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<bool> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
    Task<ProductDto> AddProductImageAsync(int productId, IFormFile imageFile);
    Task<bool> RemoveProductImageAsync(int productId, int imageId);
    Task<IEnumerable<ProductDto>> GetAllProductsAdminAsync();

    // User
    Task<IEnumerable<ProductListDto>> GetProductsUserAsync(int? categoryId, bool? isFeatured, string? sortBy, int page, int pageSize);
    Task<ProductDetailDto?> GetProductDetailAsync(int id);
    Task<IEnumerable<ProductListDto>> SearchProductsAsync(string keyword);
    Task<IEnumerable<ProductListDto>> GetSuggestionsAsync();
}
