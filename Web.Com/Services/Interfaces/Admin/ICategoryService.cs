using Web.Com.DTOs.Admin;

namespace Web.Com.Services.Interfaces.Admin;

public interface ICategoryService
{
  Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync();
  Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id);
  Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto);
  Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto);
  Task<bool> DeleteCategoryAsync(Guid id);
}

public class CategoryResponseDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? ImageUrl { get; set; }
  public bool IsActive { get; set; }
  public int ProductCount { get; set; }
  public int DisplayOrder { get; set; }
}
