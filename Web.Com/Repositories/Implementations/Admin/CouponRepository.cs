using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;

namespace Web.Com.Repositories.Implementations.Admin;

public class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _context;

    public CouponRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync()
    {
        return await _context.Coupons
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Coupon?> GetByIdAsync(int id)
    {
        return await _context.Coupons.FindAsync(id);
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code.ToUpper().Trim() && c.IsActive);
    }

    public async Task AddAsync(Coupon coupon)
    {
        await _context.Coupons.AddAsync(coupon);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Coupon coupon)
    {
        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
    }
}
