using Web.Com.DTOs.Admin;

namespace Web.Com.Services.Admin;

public interface IProductService
{
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto> AddProductImageAsync(int productId, IFormFile imageFile);
}
