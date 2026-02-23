using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.User;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] bool? isFeatured,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var products = await _productService.GetProductsUserAsync(categoryId, isFeatured, sortBy, page, pageSize);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
    {
        var product = await _productService.GetProductDetailAsync(id);
        if (product == null) return NotFound(new { message = "Product not found" });
        return Ok(product);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> SearchProducts([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return Ok(new List<ProductListDto>());
        var products = await _productService.SearchProductsAsync(keyword);
        return Ok(products);
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetSuggestions()
    {
        var products = await _productService.GetSuggestionsAsync();
        return Ok(products);
    }
}
