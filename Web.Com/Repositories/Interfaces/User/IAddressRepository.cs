using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.User;

public interface IAddressRepository
{
    Task<IEnumerable<Address>> GetByUserIdAsync(string userId);
    Task<Address?> GetByIdAsync(Guid id, string userId);
    Task AddAsync(Address address);
    Task UpdateAsync(Address address);
    Task DeleteAsync(Address address);
    Task<IEnumerable<Address>> GetDefaultsByUserIdAsync(string userId);
    Task<Address?> GetMostRecentByUserIdAsync(string userId);
    Task<bool> HasAnyAsync(string userId);
}
