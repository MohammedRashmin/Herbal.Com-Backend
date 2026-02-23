using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.Shared;

public interface IReviewService
{
    Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId);
    Task<bool> AddReviewAsync(string userId, CreateReviewDto dto);
    Task<bool> ApproveReviewAsync(int reviewId);
    Task<bool> DeleteReviewAsync(int reviewId);
}
