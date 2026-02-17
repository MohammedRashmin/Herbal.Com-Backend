using Web.Com.DTOs.Admin;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;

namespace Web.Com.Services.Admin;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IWebHostEnvironment _environment;

    public ProductService(IProductRepository productRepository, IWebHostEnvironment environment)
    {
        _productRepository = productRepository;
        _environment = environment;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            Stock = dto.Stock,
            Weight = dto.Weight,
            Ingredients = dto.Ingredients,
            CategoryId = dto.CategoryId,
            IsMemberOnly = dto.IsMemberOnly,
            IsFeatured = dto.IsFeatured
        };

        await _productRepository.CreateAsync(product);

        return MapToDto(product);
    }

    public async Task<ProductDto> AddProductImageAsync(int productId, IFormFile imageFile)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) throw new KeyNotFoundException("Product not found");

        var currentImageCount = await _productRepository.GetImageCountAsync(productId);
        if (currentImageCount >= 5) throw new InvalidOperationException("Maximum 5 images allowed per product");

        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "products");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        var imageUrl = $"/uploads/products/{fileName}";
        var productImage = new ProductImage { ProductId = productId, ImageUrl = imageUrl };
        
        await _productRepository.AddImageAsync(productImage);

        var updatedProduct = await _productRepository.GetByIdAsync(productId);
        return MapToDto(updatedProduct!);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            Stock = product.Stock,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            ImageUrls = product.Images.Select(i => i.ImageUrl).ToList()
        };
    }
}
