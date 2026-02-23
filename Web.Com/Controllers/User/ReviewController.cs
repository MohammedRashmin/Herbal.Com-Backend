using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.User;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Controllers.User;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateReview([FromBody] CreateReviewDto dto)
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _reviewService.AddReviewAsync(userId, dto);
        if (!result) return BadRequest(new { message = "Could not submit review. Please check if product exists or if you've already reviewed it." });

        return Ok(new { message = "Review submitted successfully. It will appear after admin approval." });
    }
}
