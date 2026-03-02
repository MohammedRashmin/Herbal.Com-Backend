using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
    {
        var categories = await _categoryService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponseDto>> GetCategory(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound(new { message = "Category not found" });
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var category = await _categoryService.CreateCategoryAsync(dto);
        return Ok(new { id = category.Id, message = "Category created successfully" });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        if (!result) return NotFound(new { message = "Category not found" });
        return Ok(new { message = "Category updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result) return NotFound(new { message = "Category not found" });
            return Ok(new { message = "Category deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
