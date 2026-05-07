using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _productService.GetAllProductsAdminAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productService.GetProductByIdAdminAsync(id);
        if (product == null) return NotFound(new { message = "Product not found" });
        return Ok(product);
    }
 
    [HttpPost]
    public async Task<ActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var product = await _productService.CreateProductAsync(dto);
        return Ok(new { id = product.Id, message = "Product created successfully" });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _productService.UpdateProductAsync(id, dto);
        if (!result) return NotFound(new { message = "Product not found" });
        return Ok(new { message = "Product updated successfully" });
    }

    [HttpPatch("{id}/toggle-active")]
    public async Task<ActionResult> ToggleActive(Guid id, [FromBody] bool isActive)
    {
        var result = await _productService.ToggleProductActiveAsync(id, isActive);
        if (!result) return NotFound(new { message = "Product not found" });
        return Ok(new { message = "Product status updated" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result) return NotFound(new { message = "Product not found" });
        return Ok(new { message = "Product deleted successfully" });
    }

    [HttpPost("{id}/upload-image")]
    public async Task<ActionResult> UploadImage(Guid id, IFormFile file, [FromQuery] bool isMain = false)
    {
        try
        {
            var (product, imageId) = await _productService.AddProductImageAsync(id, file, isMain);
            var returnedUrl = isMain
                ? product.ImageUrls.FirstOrDefault() ?? ""
                : product.ImageUrls.LastOrDefault() ?? "";
            return Ok(new { imageUrl = returnedUrl, imageId, message = "Image uploaded successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/remove-image/{imageId}")]
    public async Task<ActionResult> RemoveImage(Guid id, Guid imageId)
    {
        var result = await _productService.RemoveProductImageAsync(id, imageId);
        if (!result) return NotFound(new { message = "Image not found or not associated with this product" });
        return Ok(new { message = "Image removed successfully" });
    }

}
