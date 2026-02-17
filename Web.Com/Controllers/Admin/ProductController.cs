using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.Admin;
using Web.Com.Entities;
using Web.Com.Helpers.Constants;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProductController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.ShortDescription,
                p.Price,
                p.DiscountPrice,
                p.Stock,
                p.CategoryId,
                CategoryName = p.Category.Name,
                p.IsFeatured,
                p.IsMemberOnly,
                p.AverageRating,
                ImageUrls = p.Images.Select(i => i.ImageUrl).ToList()
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            return BadRequest(new { message = "Category not found" });

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

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return Ok(new { id = product.Id, message = "Product created successfully" });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            return BadRequest(new { message = "Category not found" });

        product.Name = dto.Name;
        product.ShortDescription = dto.ShortDescription;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.DiscountPrice = dto.DiscountPrice;
        product.Stock = dto.Stock;
        product.Weight = dto.Weight;
        product.Ingredients = dto.Ingredients;
        product.CategoryId = dto.CategoryId;
        product.IsMemberOnly = dto.IsMemberOnly;
        product.IsFeatured = dto.IsFeatured;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Product updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { message = "Product not found" });

        // Delete associated images from storage
        foreach (var image in product.Images)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Product deleted successfully" });
    }

    [HttpPost("{id}/upload-image")]
    public async Task<ActionResult> UploadImage(int id, IFormFile file)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { message = "Product not found" });

        if (product.Images.Count >= 5)
            return BadRequest(new { message = "Maximum 5 images allowed per product" });

        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "products");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"/uploads/products/{fileName}";
        var productImage = new ProductImage { ProductId = id, ImageUrl = imageUrl };

        _context.ProductImages.Add(productImage);
        await _context.SaveChangesAsync();

        return Ok(new { imageUrl, message = "Image uploaded successfully" });
    }

    [HttpDelete("{id}/remove-image/{imageId}")]
    public async Task<ActionResult> RemoveImage(int id, int imageId)
    {
        var image = await _context.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == id);

        if (image == null)
            return NotFound(new { message = "Image not found" });

        // Delete file from storage
        var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", image.ImageUrl.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Image removed successfully" });
    }
}
