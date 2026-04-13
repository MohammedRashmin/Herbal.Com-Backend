using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;

namespace Web.Com.Repositories.Implementations.Admin;

public class ProductRepository : IProductRepository
{
  private readonly AppDbContext _context;

  public ProductRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<Product> CreateAsync(Product product)
  {
    await _context.Products.AddAsync(product);
    await _context.SaveChangesAsync();
    return product;
  }

  public async Task<Product?> GetByIdAsync(Guid id)
  {
    return await _context
      .Products.Include(p => p.Category)
      .Include(p => p.Images)
      .FirstOrDefaultAsync(p => p.Id == id);
  }

  public async Task<IEnumerable<Product>> GetAllAsync()
  {
    return await _context.Products.Include(p => p.Category).Include(p => p.Images).ToListAsync();
  }

  public async Task UpdateAsync(Product product)
  {
    _context.Products.Update(product);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteAsync(Product product)
  {
    _context.Products.Remove(product);
    await _context.SaveChangesAsync();
  }

  public async Task AddImageAsync(ProductImage image)
  {
    await _context.ProductImages.AddAsync(image);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteImageAsync(ProductImage image)
  {
    _context.ProductImages.Remove(image);
    await _context.SaveChangesAsync();
  }

  public async Task<ProductImage?> GetImageByIdAsync(Guid imageId)
  {
    return await _context.ProductImages.FindAsync(imageId);
  }

  public async Task<int> GetImageCountAsync(Guid productId)
  {
    return await _context.ProductImages.CountAsync(i => i.ProductId == productId);
  }

  public async Task ClearMainFlagAsync(Guid productId)
  {
    var mainImages = await _context.ProductImages
      .Where(i => i.ProductId == productId && i.IsMain)
      .ToListAsync();
    foreach (var img in mainImages)
      img.IsMain = false;
    await _context.SaveChangesAsync();
  }

  public async Task<IEnumerable<Product>> SearchAsync(string keyword, int limit)
  {
    return await _context
      .Products.Include(p => p.Category)
      .Include(p => p.Images)
      .Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword))
      .Take(limit)
      .ToListAsync();
  }
}
