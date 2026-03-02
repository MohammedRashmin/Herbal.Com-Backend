using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.User;
using Web.Com.Services.Interfaces.User;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WishlistItemDto>>> GetWishlist()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var wishlist = await _wishlistService.GetWishlistAsync(userId);
        return Ok(wishlist);
    }

    [HttpPost("{productId}")]
    public async Task<ActionResult> AddToWishlist(Guid productId)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _wishlistService.AddToWishlistAsync(userId, productId);

        if (!result)
            return BadRequest(new { message = "Product not found or already in wishlist" });

        return Ok(new { message = "Product added to wishlist" });
    }

    [HttpDelete("{productId}")]
    public async Task<ActionResult> RemoveFromWishlist(Guid productId)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId);

        if (!result)
            return NotFound(new { message = "Product not found in wishlist" });

        return Ok(new { message = "Product removed from wishlist" });
    }

    [HttpGet("check/{productId}")]
    public async Task<ActionResult<bool>> IsInWishlist(Guid productId)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, productId);
        return Ok(new { isInWishlist });
    }
}
