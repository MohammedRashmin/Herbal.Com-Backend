using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.User;

namespace Web.Com.Repositories.Implementations.User;

public class WishlistRepository : IWishlistRepository
{
    private readonly AppDbContext _context;

    public WishlistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId)
    {
        return await _context.WishlistItems
            .Include(w => w.Product)
                .ThenInclude(p => p.Images)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<WishlistItem?> GetAsync(string userId, Guid productId)
    {
        return await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
    }

    public async Task AddAsync(WishlistItem item)
    {
        await _context.WishlistItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(WishlistItem item)
    {
        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string userId, Guid productId)
    {
        return await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
    }
}
