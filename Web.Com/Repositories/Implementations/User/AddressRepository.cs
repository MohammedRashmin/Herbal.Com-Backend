using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.User;

namespace Web.Com.Repositories.Implementations.User;

public class AddressRepository : IAddressRepository
{
    private readonly AppDbContext _context;

    public AddressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Address>> GetByUserIdAsync(string userId)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Address?> GetByIdAsync(int id, string userId)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
    }

    public async Task AddAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Address address)
    {
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Address address)
    {
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Address>> GetDefaultsByUserIdAsync(string userId)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ToListAsync();
    }

    public async Task<Address?> GetMostRecentByUserIdAsync(string userId)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasAnyAsync(string userId)
    {
        return await _context.Addresses.AnyAsync(a => a.UserId == userId);
    }
}
