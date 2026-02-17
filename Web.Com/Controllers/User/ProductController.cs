using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.User;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] bool? isFeatured,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.Stock > 0)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (isFeatured.HasValue && isFeatured.Value)
            query = query.Where(p => p.IsFeatured);

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
            "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
            "name" => query.OrderBy(p => p.Name),
            "rating" => query.OrderByDescending(p => p.AverageRating),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var products = await query
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
                ImageUrl = p.Images.FirstOrDefault() != null ? p.Images.First().ImageUrl : null,
                CategoryName = p.Category.Name,
                IsFeatured = p.IsFeatured,
                IsMemberOnly = p.IsMemberOnly
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { message = "Product not found" });

        var dto = new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            Stock = product.Stock,
            Weight = product.Weight,
            Ingredients = product.Ingredients,
            AverageRating = product.AverageRating,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            IsFeatured = product.IsFeatured,
            IsMemberOnly = product.IsMemberOnly,
            ImageUrls = product.Images.Select(i => i.ImageUrl).ToList(),
            Reviews = product.Reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> SearchProducts([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Ok(new List<ProductListDto>());

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword))
            .Take(20)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                ShortDescription = p.ShortDescription,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                AverageRating = p.AverageRating,
                Stock = p.Stock,
                ImageUrl = p.Images.FirstOrDefault() != null ? p.Images.First().ImageUrl : null,
                CategoryName = p.Category.Name,
                IsFeatured = p.IsFeatured,
                IsMemberOnly = p.IsMemberOnly
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetSuggestions()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
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
                ImageUrl = p.Images.FirstOrDefault() != null ? p.Images.First().ImageUrl : null,
                CategoryName = p.Category.Name,
                IsFeatured = p.IsFeatured,
                IsMemberOnly = p.IsMemberOnly
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}/reviews")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int id)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == id && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(reviews);
    }
}
