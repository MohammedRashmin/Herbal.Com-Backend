using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.User;
using Web.Com.Entities;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        var cartItems = await _context.CartItems
            .Include(c => c.Product)
                .ThenInclude(p => p.Images)
            .Where(c => c.UserId == userId)
            .Select(c => new CartItemDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                ProductImageUrl = c.Product.Images.FirstOrDefault() != null ? c.Product.Images.First().ImageUrl : null,
                Price = c.Product.Price,
                DiscountPrice = c.Product.DiscountPrice,
                Quantity = c.Quantity,
                Stock = c.Product.Stock
            })
            .ToListAsync();

        return Ok(cartItems);
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        if (product.Stock < dto.Quantity)
            return BadRequest(new { message = "Insufficient stock" });

        // Check if item already in cart
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            if (existingItem.Quantity > product.Stock)
                return BadRequest(new { message = "Quantity exceeds available stock" });
        }
        else
        {
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Item added to cart" });
    }

    [HttpPut("update")]
    public async Task<ActionResult> UpdateCartItem([FromBody] UpdateCartItemDto dto)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        var cartItem = await _context.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == dto.CartItemId && c.UserId == userId);

        if (cartItem == null)
            return NotFound(new { message = "Cart item not found" });

        if (dto.Quantity <= 0)
        {
            _context.CartItems.Remove(cartItem);
        }
        else if (dto.Quantity > cartItem.Product.Stock)
        {
            return BadRequest(new { message = "Quantity exceeds available stock" });
        }
        else
        {
            cartItem.Quantity = dto.Quantity;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cart updated" });
    }

    [HttpDelete("remove/{itemId}")]
    public async Task<ActionResult> RemoveFromCart(int itemId)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId);

        if (cartItem == null)
            return NotFound(new { message = "Cart item not found" });

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Item removed from cart" });
    }

    [HttpDelete("clear")]
    public async Task<ActionResult> ClearCart()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cart cleared" });
    }
}
