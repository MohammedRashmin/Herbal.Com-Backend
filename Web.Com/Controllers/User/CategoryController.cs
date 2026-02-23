using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.User;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
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
        return Ok(categories.Where(c => c.IsActive));
    }
}
