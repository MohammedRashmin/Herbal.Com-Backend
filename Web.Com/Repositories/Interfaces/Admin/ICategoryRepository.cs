using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.Admin;

public interface ICategoryRepository
{
  Task<IEnumerable<Category>> GetAllAsync();
  Task<Category?> GetByIdAsync(Guid id);
  Task AddAsync(Category category);
  Task UpdateAsync(Category category);
  Task DeleteAsync(Category category);
  Task<bool> HasProductsAsync(Guid categoryId);
}
