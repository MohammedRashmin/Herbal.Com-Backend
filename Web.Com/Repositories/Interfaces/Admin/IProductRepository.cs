using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.Admin;

public interface IProductRepository
{
  Task<IEnumerable<Product>> GetAllAsync();
  Task<Product?> GetByIdAsync(Guid id);
  Task<Product> CreateAsync(Product product);
  Task UpdateAsync(Product product);
  Task DeleteAsync(Product product);
  Task AddImageAsync(ProductImage image);
  Task DeleteImageAsync(ProductImage image);
  Task<ProductImage?> GetImageByIdAsync(Guid imageId);
  Task<int> GetImageCountAsync(Guid productId);
  Task ClearMainFlagAsync(Guid productId);
  Task<IEnumerable<Product>> SearchAsync(string keyword, int limit);
}
