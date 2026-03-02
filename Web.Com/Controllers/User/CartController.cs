using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.User;
using Web.Com.Services.Interfaces.User;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        var cartItems = await _cartService.GetCartAsync(userId);
        return Ok(cartItems);
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        try
        {
            await _cartService.AddToCartAsync(userId, dto);
            return Ok(new { message = "Item added to cart" });
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

    [HttpPut("update")]
    public async Task<ActionResult> UpdateCartItem([FromBody] UpdateCartItemDto dto)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        try
        {
            await _cartService.UpdateCartItemAsync(dto.CartItemId, dto);
            return Ok(new { message = "Cart updated" });
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

    [HttpDelete("remove/{itemId}")]
    public async Task<ActionResult> RemoveFromCart(Guid itemId)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        await _cartService.RemoveFromCartAsync(itemId);
        return Ok(new { message = "Item removed from cart" });
    }

    [HttpDelete("clear")]
    public async Task<ActionResult> ClearCart()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) 
            return Unauthorized();

        await _cartService.ClearCartAsync(userId);
        return Ok(new { message = "Cart cleared" });
    }
}
