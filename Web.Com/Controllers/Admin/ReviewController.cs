using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.Admin;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminReviewDto>>> GetAllReviews()
    {
        var reviews = await _reviewService.GetAllReviewsAsync();
        return Ok(reviews);
    }

    [HttpPut("{id}/reject")]
    public async Task<ActionResult> RejectReview(Guid id)
    {
        var result = await _reviewService.RejectReviewAsync(id);
        if (!result) return NotFound(new { message = "Review not found" });
        return Ok(new { message = "Review rejected" });
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult> ApproveReview(Guid id)
    {
        var result = await _reviewService.ApproveReviewAsync(id);
        if (!result) return NotFound(new { message = "Review not found" });
        return Ok(new { message = "Review approved" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReview(Guid id)
    {
        var result = await _reviewService.DeleteReviewAsync(id);
        if (!result) return NotFound(new { message = "Review not found" });
        return Ok(new { message = "Review deleted" });
    }

}
