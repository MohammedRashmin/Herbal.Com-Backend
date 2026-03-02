using Web.Com.DTOs.Admin;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Services.Implementations.Admin;

public class CategoryService : ICategoryService
{
  private readonly ICategoryRepository _categoryRepository;

  public CategoryService(ICategoryRepository categoryRepository)
  {
    _categoryRepository = categoryRepository;
  }

  public async Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync()
  {
    var categories = await _categoryRepository.GetAllAsync();
    return categories.Select(c => new CategoryResponseDto
    {
      Id = c.Id,
      Name = c.Name,
      Description = c.Description,
      ImageUrl = c.ImageUrl,
      IsActive = c.IsActive,
      ProductCount = c.Products.Count,
      DisplayOrder = c.DisplayOrder,
    });
  }

  public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id)
  {
    var c = await _categoryRepository.GetByIdAsync(id);
    if (c == null) return null;
    return new CategoryResponseDto
    {
      Id = c.Id,
      Name = c.Name,
      Description = c.Description,
      ImageUrl = c.ImageUrl,
      IsActive = c.IsActive,
      ProductCount = c.Products?.Count ?? 0,
      DisplayOrder = c.DisplayOrder,
    };
  }

  public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto)
  {
    var category = new Category
    {
      Name = dto.Name,
      Description = dto.Description,
      ImageUrl = dto.ImageUrl,
      DisplayOrder = dto.DisplayOrder,
      IsActive = dto.IsActive,
    };

    await _categoryRepository.AddAsync(category);

    return new CategoryResponseDto
    {
      Id = category.Id,
      Name = category.Name,
      Description = category.Description,
      ImageUrl = category.ImageUrl,
      IsActive = category.IsActive,
    };
  }

  public async Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto)
  {
    var category = await _categoryRepository.GetByIdAsync(id);
    if (category == null)
      return false;

    category.Name = dto.Name;
    category.Description = dto.Description;
    category.ImageUrl = dto.ImageUrl;
    category.DisplayOrder = dto.DisplayOrder;
    category.IsActive = dto.IsActive;

    await _categoryRepository.UpdateAsync(category);
    return true;
  }

  public async Task<bool> DeleteCategoryAsync(Guid id)
  {
    var category = await _categoryRepository.GetByIdAsync(id);
    if (category == null)
      return false;

    var hasProducts = await _categoryRepository.HasProductsAsync(id);
    if (hasProducts)
      throw new InvalidOperationException("Cannot delete category with products.");

    await _categoryRepository.DeleteAsync(category);
    return true;
  }
}
