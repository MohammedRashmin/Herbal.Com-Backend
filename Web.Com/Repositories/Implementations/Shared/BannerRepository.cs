using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Shared;

namespace Web.Com.Repositories.Implementations.Shared;

public class BannerRepository : IBannerRepository
{
    private readonly AppDbContext _context;

    public BannerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Banner>> GetAllAsync()
    {
        return await _context.Banners
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Banner>> GetActiveAsync()
    {
        return await _context.Banners
            .Where(b => b.IsActive && (b.EndDate == null || b.EndDate > DateTime.UtcNow))
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Banner?> GetByIdAsync(Guid id)
    {
        return await _context.Banners.FindAsync(id);
    }

    public async Task AddAsync(Banner banner)
    {
        await _context.Banners.AddAsync(banner);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Banner banner)
    {
        _context.Banners.Update(banner);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Banner banner)
    {
        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync();
    }
}
