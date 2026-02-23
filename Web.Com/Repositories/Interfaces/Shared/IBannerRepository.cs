using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.Shared;

public interface IBannerRepository
{
    Task<IEnumerable<Banner>> GetAllAsync();
    Task<IEnumerable<Banner>> GetActiveAsync();
    Task<Banner?> GetByIdAsync(int id);
    Task AddAsync(Banner banner);
    Task UpdateAsync(Banner banner);
    Task DeleteAsync(Banner banner);
}
