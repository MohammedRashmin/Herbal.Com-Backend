using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Shared;

namespace Web.Com.Repositories.Implementations.Shared;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetAllAsync()
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId, bool approvedOnly = true)
    {
        var query = _context.Reviews.Include(r => r.User).Where(r => r.ProductId == productId);
        if (approvedOnly) query = query.Where(r => r.IsApproved);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(Guid id)
    {
        return await _context.Reviews.FindAsync(id);
    }

    public async Task AddAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Review review)
    {
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> RejectAsync(Guid reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null) return false;
        review.IsRejected = true;
        review.IsApproved = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RecalculateProductRatingAsync(Guid productId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return;

        var approvedRatings = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Select(r => r.Rating)
            .ToListAsync();

        product.AverageRating = approvedRatings.Count > 0
            ? Math.Round((decimal)approvedRatings.Average(), 1)
            : 0;

        await _context.SaveChangesAsync();
    }

}
