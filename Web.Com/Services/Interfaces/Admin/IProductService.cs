using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.Admin;

public interface IProductService
{
  // Admin
  Task<ProductDto> CreateProductAsync(CreateProductDto dto);
  Task<bool> UpdateProductAsync(Guid id, UpdateProductDto dto);
  Task<bool> DeleteProductAsync(Guid id);
  Task<ProductDto> AddProductImageAsync(Guid productId, IFormFile imageFile, bool isMain = false);
  Task<bool> RemoveProductImageAsync(Guid productId, Guid imageId);
  Task<IEnumerable<ProductDto>> GetAllProductsAdminAsync();

  // User
  Task<IEnumerable<ProductListDto>> GetProductsUserAsync(
    Guid? categoryId,
    bool? isFeatured,
    string? keyword,
    string? sortBy,
    int page,
    int pageSize
  );
  Task<ProductDetailDto?> GetProductDetailAsync(Guid id);
  Task<IEnumerable<ProductListDto>> GetSuggestionsAsync();
}
