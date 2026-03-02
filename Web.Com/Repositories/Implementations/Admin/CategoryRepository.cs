using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;

namespace Web.Com.Repositories.Implementations.Admin;

public class CategoryRepository : ICategoryRepository
{
  private readonly AppDbContext _context;

  public CategoryRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<Category>> GetAllAsync()
  {
    return await _context.Categories.Include(c => c.Products).OrderBy(c => c.Name).ToListAsync();
  }

  public async Task<Category?> GetByIdAsync(Guid id)
  {
    return await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
  }

  public async Task AddAsync(Category category)
  {
    await _context.Categories.AddAsync(category);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateAsync(Category category)
  {
    _context.Categories.Update(category);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteAsync(Category category)
  {
    _context.Categories.Remove(category);
    await _context.SaveChangesAsync();
  }

  public async Task<bool> HasProductsAsync(Guid categoryId)
  {
    return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
  }
}
