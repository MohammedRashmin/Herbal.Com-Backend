using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.Admin;

public interface IProductRepository
{
    Task<Product> CreateAsync(Product product);
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task AddImageAsync(ProductImage image);
    Task<int> GetImageCountAsync(int productId);
}
