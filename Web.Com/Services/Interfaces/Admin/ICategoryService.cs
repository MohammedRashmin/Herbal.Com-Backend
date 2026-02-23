using Web.Com.DTOs.Admin;

namespace Web.Com.Services.Interfaces.Admin;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync();
    Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
}

public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}
