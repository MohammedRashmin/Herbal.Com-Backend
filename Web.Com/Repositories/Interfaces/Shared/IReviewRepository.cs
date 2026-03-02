using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.Shared;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId, bool approvedOnly = true);
    Task<Review?> GetByIdAsync(Guid id);
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Review review);
}
