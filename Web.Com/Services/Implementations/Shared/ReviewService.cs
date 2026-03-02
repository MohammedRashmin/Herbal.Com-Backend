using Web.Com.DTOs.User;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Shared;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Services.Implementations.Shared;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(Guid productId)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId);
        return reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            UserName = $"{r.User?.FirstName} {r.User?.LastName}",
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        });
    }

    public async Task<bool> AddReviewAsync(string userId, CreateReviewDto dto)
    {
        var review = new Review
        {
            UserId = userId,
            ProductId = dto.ProductId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            IsApproved = false // Pending approval
        };

        await _reviewRepository.AddAsync(review);
        return true;
    }

    public async Task<bool> ApproveReviewAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return false;

        review.IsApproved = true;
        await _reviewRepository.UpdateAsync(review);
        return true;
    }

    public async Task<bool> DeleteReviewAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return false;

        await _reviewRepository.DeleteAsync(review);
        return true;
    }
}
