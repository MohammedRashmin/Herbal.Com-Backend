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

    public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId, bool approvedOnly = true)
    {
        var query = _context.Reviews.Include(r => r.User).Where(r => r.ProductId == productId);
        if (approvedOnly) query = query.Where(r => r.IsApproved);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<Review?> GetByIdAsync(int id)
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
}
