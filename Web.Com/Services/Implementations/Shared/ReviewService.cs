using Web.Com.DTOs.Admin;
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

    public async Task<IEnumerable<AdminReviewDto>> GetAllReviewsAsync()
    {
        var reviews = await _reviewRepository.GetAllAsync();
        return reviews.Select(r => new AdminReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.Product?.Name ?? "",
            UserName = $"{r.User?.FirstName} {r.User?.LastName}".Trim(),
            Rating = r.Rating,
            Comment = r.Comment,
            IsApproved = r.IsApproved,
            IsRejected = r.IsRejected,
            CreatedAt = r.CreatedAt
        });
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

    public async Task<bool> RejectReviewAsync(Guid reviewId)
    {
        return await _reviewRepository.RejectAsync(reviewId);
    }

    public async Task<bool> ApproveReviewAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return false;

        review.IsApproved = true;
        await _reviewRepository.UpdateAsync(review);
        await _reviewRepository.RecalculateProductRatingAsync(review.ProductId);
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
