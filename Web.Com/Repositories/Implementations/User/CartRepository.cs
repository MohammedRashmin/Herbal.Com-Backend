using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.User;

namespace Web.Com.Repositories.Implementations.User;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId)
    {
        return await _context.CartItems
            .Include(c => c.Product)
            .ThenInclude(p => p.Images)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<CartItem?> GetCartItemAsync(string userId, int productId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
    }

    public async Task AddCartItemAsync(CartItem cartItem)
    {
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCartItemAsync(CartItem cartItem)
    {
        _context.CartItems.Update(cartItem);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveCartItemAsync(int cartItemId)
    {
        var cartItem = await _context.CartItems.FindAsync(cartItemId);
        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CartItem?> GetByIdAsync(int id)
    {
        return await _context.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task ClearCartAsync(string userId)
    {
        var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}
