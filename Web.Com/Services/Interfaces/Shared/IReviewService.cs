using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.Shared;

public interface IReviewService
{
    Task<IEnumerable<AdminReviewDto>> GetAllReviewsAsync();
    Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(Guid productId);
    Task<bool> AddReviewAsync(string userId, CreateReviewDto dto);
    Task<bool> ApproveReviewAsync(Guid reviewId);
    Task<bool> RejectReviewAsync(Guid reviewId);
    Task<bool> DeleteReviewAsync(Guid reviewId);
}
