using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Web.Com.DTOs;
using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;
using Web.Com.Entities;
using Web.Com.Helpers;
using Web.Com.Repositories.Interfaces.Admin;
using Web.Com.Services.Interfaces.Admin;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Services.Implementations.Admin;

public class ProductService : IProductService
{
  private readonly IProductRepository _productRepository;
  private readonly IWebHostEnvironment _environment;
  private readonly IPhotoService _photoService;

  public ProductService(
    IProductRepository productRepository,
    IWebHostEnvironment environment,
    IPhotoService photoService)
  {
    _productRepository = productRepository;
    _environment = environment;
    _photoService = photoService;
  }

  public async Task<IEnumerable<ProductDto>> GetAllProductsAdminAsync()
  {
    var products = await _productRepository.GetAllAsync();
    return products.Select(MapToDto);
  }

  public async Task<ProductDto?> GetProductByIdAdminAsync(Guid id)
  {
    var product = await _productRepository.GetByIdAsync(id);
    return product == null ? null : MapToDto(product);
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
      ServingSize = dto.ServingSize,
      ServingsPerContainer = dto.ServingsPerContainer,
      Badges = dto.Badges != null ? JsonSerializer.Serialize(dto.Badges) : null,
      Benefits = dto.Benefits != null ? JsonSerializer.Serialize(dto.Benefits) : null,
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
    product.IsActive = dto.IsActive;
    product.Sku = dto.Sku;
    product.ExpiryDate = dto.ExpiryDate;
    product.ServingSize = dto.ServingSize;
    product.ServingsPerContainer = dto.ServingsPerContainer;
    product.Badges = dto.Badges != null ? JsonSerializer.Serialize(dto.Badges) : null;
    product.Benefits = dto.Benefits != null ? JsonSerializer.Serialize(dto.Benefits) : null;

    await _productRepository.UpdateAsync(product);
    return true;
  }

  public async Task<bool> DeleteProductAsync(Guid id)
  {
    var product = await _productRepository.GetByIdAsync(id);
    if (product == null)
      return false;

    // Delete images: Cloudinary or local file
    foreach (var image in product.Images)
    {
      if (!string.IsNullOrEmpty(image.CloudinaryPublicId))
        _ = await _photoService.DeletePhotoAsync(image.CloudinaryPublicId);
      else if (image.ImageUrl.StartsWith("/"))
      {
        var filePath = Path.Combine(
          _environment.ContentRootPath,
          "wwwroot",
          image.ImageUrl.TrimStart('/')
        );
        if (File.Exists(filePath))
          File.Delete(filePath);
      }
    }

    await _productRepository.DeleteAsync(product);
    return true;
  }

  public async Task<(ProductDto Product, Guid ImageId)> AddProductImageAsync(Guid productId, IFormFile imageFile, bool isMain = false)
  {
    var product = await _productRepository.GetByIdAsync(productId);
    if (product == null)
      throw new KeyNotFoundException("Product not found");

    // When replacing the main image, remove the old main first so it doesn't count toward the 5-image limit
    if (isMain)
    {
      var existingMain = product.Images?.FirstOrDefault(i => i.IsMain);
      if (existingMain != null)
        await RemoveProductImageAsync(productId, existingMain.Id);
    }

    var count = await _productRepository.GetImageCountAsync(productId);
    if (count >= 5)
      throw new InvalidOperationException("Maximum 5 images allowed per product");

    var result = await _photoService.AddPhotoAsync(imageFile);
    if (result.Error != null)
      throw new InvalidOperationException(result.Error.Message);

    if (isMain)
      await _productRepository.ClearMainFlagAsync(productId);

    var imageUrl = result.SecureUrl.AbsoluteUri;
    var newImage = new ProductImage
    {
      ProductId = productId,
      ImageUrl = imageUrl,
      CloudinaryPublicId = result.PublicId,
      IsMain = isMain
    };
    await _productRepository.AddImageAsync(newImage);

    var updatedProduct = await _productRepository.GetByIdAsync(productId);
    return (MapToDto(updatedProduct!), newImage.Id);
  }

  public async Task<bool> RemoveProductImageAsync(Guid productId, Guid imageId)
  {
    var image = await _productRepository.GetImageByIdAsync(imageId);
    if (image == null || image.ProductId != productId)
      return false;

    if (!string.IsNullOrEmpty(image.CloudinaryPublicId))
      _ = await _photoService.DeletePhotoAsync(image.CloudinaryPublicId);
    else if (image.ImageUrl.StartsWith("/"))
    {
      var filePath = Path.Combine(
        _environment.ContentRootPath,
        "wwwroot",
        image.ImageUrl.TrimStart('/')
      );
      if (File.Exists(filePath))
        File.Delete(filePath);
    }

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
    var query = products.Where(p => p.IsActive);

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
        ImageUrl = CloudinaryUrlHelper.ToDeliveryUrl(
          (p.Images.FirstOrDefault(i => i.IsMain) ?? p.Images.FirstOrDefault())?.ImageUrl),
        CategoryName = p.Category?.Name ?? "Uncategorized",
        IsFeatured = p.IsFeatured,
        IsMemberOnly = p.IsMemberOnly,
        Sku = p.Sku,
        ExpiryDate = p.ExpiryDate,
        Weight = p.Weight,
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
      ServingSize = p.ServingSize,
      ServingsPerContainer = p.ServingsPerContainer,
      Badges = string.IsNullOrEmpty(p.Badges) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(p.Badges)!,
      Benefits = string.IsNullOrEmpty(p.Benefits) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(p.Benefits)!,
      ProductImages = p.Images?
        .OrderByDescending(i => i.IsMain)
        .Select(i => new ProductImageItemDto
        {
          Id = i.Id,
          Url = CloudinaryUrlHelper.ToDeliveryUrl(i.ImageUrl),
          IsMain = i.IsMain
        }).ToList() ?? new List<ProductImageItemDto>(),
      ImageUrls = p.Images.Select(i => CloudinaryUrlHelper.ToDeliveryUrl(i.ImageUrl)).ToList(),
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
        ImageUrl = CloudinaryUrlHelper.ToDeliveryUrl(
          (p.Images.FirstOrDefault(i => i.IsMain) ?? p.Images.FirstOrDefault())?.ImageUrl),
        CategoryName = p.Category?.Name ?? "Uncategorized",
        IsFeatured = p.IsFeatured,
        IsMemberOnly = p.IsMemberOnly,
        Weight = p.Weight,
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
      ServingSize = p.ServingSize,
      ServingsPerContainer = p.ServingsPerContainer,
      Badges = string.IsNullOrEmpty(p.Badges) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(p.Badges)!,
      Benefits = string.IsNullOrEmpty(p.Benefits) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(p.Benefits)!,
      ImageUrls = p.Images?
        .OrderByDescending(i => i.IsMain)
        .Select(i => CloudinaryUrlHelper.ToDeliveryUrl(i.ImageUrl))
        .ToList() ?? new List<string>(),
    };
  }
}
