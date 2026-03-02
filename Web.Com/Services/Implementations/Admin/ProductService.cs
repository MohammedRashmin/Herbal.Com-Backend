using Microsoft.EntityFrameworkCore;
using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Services.Implementations.Admin;

public class ProductService : IProductService
{
  private readonly IProductRepository _productRepository;
  private readonly IWebHostEnvironment _environment;

  public ProductService(IProductRepository productRepository, IWebHostEnvironment environment)
  {
    _productRepository = productRepository;
    _environment = environment;
  }

  public async Task<IEnumerable<ProductDto>> GetAllProductsAdminAsync()
  {
    var products = await _productRepository.GetAllAsync();
    return products.Select(MapToDto);
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
      BatchNumber = dto.BatchNumber,
      Ingredients = dto.Ingredients,
      CategoryId = dto.CategoryId,
      IsMemberOnly = dto.IsMemberOnly,
      IsFeatured = dto.IsFeatured,
      Sku = dto.Sku,
      ExpiryDate = dto.ExpiryDate,
    };

    var createdProduct = await _productRepository.CreateAsync(product);
    return MapToDto(createdProduct);
  }

  public async Task<bool> UpdateProductAsync(Guid id, UpdateProductDto dto)
  {
    var product = await _productRepository.GetByIdAsync(id);
    if (product == null)
      return false;

    product.Name = dto.Name;
    product.ShortDescription = dto.ShortDescription;
    product.Description = dto.Description;
    product.Price = dto.Price;
    product.DiscountPrice = dto.DiscountPrice;
    product.Stock = dto.Stock;
    product.Weight = dto.Weight;
    product.BatchNumber = dto.BatchNumber;
    product.Ingredients = dto.Ingredients;
    product.CategoryId = dto.CategoryId;
    product.IsMemberOnly = dto.IsMemberOnly;
    product.IsFeatured = dto.IsFeatured;
    product.Sku = dto.Sku;
    product.ExpiryDate = dto.ExpiryDate;

    await _productRepository.UpdateAsync(product);
    return true;
  }

  public async Task<bool> DeleteProductAsync(Guid id)
  {
    var product = await _productRepository.GetByIdAsync(id);
    if (product == null)
      return false;

    // Delete files
    foreach (var image in product.Images)
    {
      var filePath = Path.Combine(
        _environment.ContentRootPath,
        "wwwroot",
        image.ImageUrl.TrimStart('/')
      );
      if (File.Exists(filePath))
        File.Delete(filePath);
    }

    await _productRepository.DeleteAsync(product);
    return true;
  }

  public async Task<ProductDto> AddProductImageAsync(Guid productId, IFormFile imageFile)
  {
    var product = await _productRepository.GetByIdAsync(productId);
    if (product == null)
      throw new KeyNotFoundException("Product not found");

    var count = await _productRepository.GetImageCountAsync(productId);
    if (count >= 5)
      throw new InvalidOperationException("Maximum 5 images allowed per product");

    var uploadsFolder = Path.Combine(
      _environment.ContentRootPath,
      "wwwroot",
      "uploads",
      "products"
    );
    if (!Directory.Exists(uploadsFolder))
      Directory.CreateDirectory(uploadsFolder);

    var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
    var filePath = Path.Combine(uploadsFolder, fileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
      await imageFile.CopyToAsync(stream);
    }

    var imageUrl = $"/uploads/products/{fileName}";
    await _productRepository.AddImageAsync(
      new ProductImage { ProductId = productId, ImageUrl = imageUrl }
    );

    var updatedProduct = await _productRepository.GetByIdAsync(productId);
    return MapToDto(updatedProduct!);
  }

  public async Task<bool> RemoveProductImageAsync(Guid productId, Guid imageId)
  {
    var image = await _productRepository.GetImageByIdAsync(imageId);
    if (image == null || image.ProductId != productId)
      return false;

    var filePath = Path.Combine(
      _environment.ContentRootPath,
      "wwwroot",
      image.ImageUrl.TrimStart('/')
    );
    if (File.Exists(filePath))
      File.Delete(filePath);

    await _productRepository.DeleteImageAsync(image);
    return true;
  }

  // User Methods
  public async Task<IEnumerable<ProductListDto>> GetProductsUserAsync(
    Guid? categoryId,
    bool? isFeatured,
    string? keyword,
    string? sortBy,
    int page,
    int pageSize
  )
  {
    var products = await _productRepository.GetAllAsync();
    var query = products.Where(p => p.Stock > 0);

    if (categoryId.HasValue)
      query = query.Where(p => p.CategoryId == categoryId);
    if (isFeatured.HasValue)
      query = query.Where(p => p.IsFeatured == isFeatured.Value);
    if (!string.IsNullOrWhiteSpace(keyword))
    {
      var k = keyword.ToLower();
      query = query.Where(p =>
        p.Name.ToLower().Contains(k)
        || (p.Description != null && p.Description.ToLower().Contains(k))
      );
    }

    query = sortBy?.ToLower() switch
    {
      "price_asc" => query.OrderBy(p => (p.DiscountPrice ?? p.Price)),
      "price_desc" => query.OrderByDescending(p => (p.DiscountPrice ?? p.Price)),
      "name" => query.OrderBy(p => p.Name),
      "rating" => query.OrderByDescending(p => p.AverageRating),
      _ => query.OrderByDescending(p => p.CreatedAt),
    };

    return query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Select(p => new ProductListDto
      {
        Id = p.Id,
        Name = p.Name,
        ShortDescription = p.ShortDescription,
        Price = p.Price,
        DiscountPrice = p.DiscountPrice,
        AverageRating = p.AverageRating,
        Stock = p.Stock,
        ImageUrl = p.Images.FirstOrDefault()?.ImageUrl,
        CategoryName = p.Category?.Name ?? "Uncategorized",
        IsFeatured = p.IsFeatured,
        IsMemberOnly = p.IsMemberOnly,
        Sku = p.Sku,
        ExpiryDate = p.ExpiryDate,
      });
  }

  public async Task<ProductDetailDto?> GetProductDetailAsync(Guid id)
  {
    var p = await _productRepository.GetByIdAsync(id);
    if (p == null)
      return null;

    return new ProductDetailDto
    {
      Id = p.Id,
      Name = p.Name,
      ShortDescription = p.ShortDescription,
      Description = p.Description,
      Price = p.Price,
      DiscountPrice = p.DiscountPrice,
      Stock = p.Stock,
      Weight = p.Weight,
      Ingredients = p.Ingredients,
      AverageRating = p.AverageRating,
      CategoryId = p.CategoryId,
      CategoryName = p.Category?.Name ?? "Uncategorized",
      IsFeatured = p.IsFeatured,
      IsMemberOnly = p.IsMemberOnly,
      Sku = p.Sku,
      ExpiryDate = p.ExpiryDate,
      ImageUrls = p.Images.Select(i => i.ImageUrl).ToList(),
      Reviews =
        p.Reviews?.Where(r => r.IsApproved)
          .Select(r => new ReviewDto
          {
            Id = r.Id,
            UserName = $"{r.User?.FirstName} {r.User?.LastName}",
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
          })
          .ToList() ?? new List<ReviewDto>(),
    };
  }

  public async Task<IEnumerable<ProductListDto>> GetSuggestionsAsync()
  {
    var products = await _productRepository.GetAllAsync();
    return products
      .Where(p => p.Stock > 0)
      .OrderByDescending(p => p.AverageRating)
      .ThenByDescending(p => p.CreatedAt)
      .Take(8)
      .Select(p => new ProductListDto
      {
        Id = p.Id,
        Name = p.Name,
        ShortDescription = p.ShortDescription,
        Price = p.Price,
        DiscountPrice = p.DiscountPrice,
        AverageRating = p.AverageRating,
        Stock = p.Stock,
        ImageUrl = p.Images.FirstOrDefault()?.ImageUrl,
        CategoryName = p.Category?.Name ?? "Uncategorized",
        IsFeatured = p.IsFeatured,
        IsMemberOnly = p.IsMemberOnly,
      });
  }

  private static ProductDto MapToDto(Product p)
  {
    return new ProductDto
    {
      Id = p.Id,
      Name = p.Name,
      ShortDescription = p.ShortDescription,
      Description = p.Description,
      Price = p.Price,
      DiscountPrice = p.DiscountPrice,
      Stock = p.Stock,
      Weight = p.Weight,
      BatchNumber = p.BatchNumber,
      Ingredients = p.Ingredients,
      CategoryId = p.CategoryId,
      CategoryName = p.Category?.Name ?? "Uncategorized",
      IsFeatured = p.IsFeatured,
      IsMemberOnly = p.IsMemberOnly,
      Sku = p.Sku,
      ExpiryDate = p.ExpiryDate,
      ImageUrls = p.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
    };
  }
}
