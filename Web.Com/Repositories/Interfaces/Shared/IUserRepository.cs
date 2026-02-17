using Web.Com.Entities.Identity;

namespace Web.Com.Repositories.Interfaces.Shared;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser> CreateAsync(ApplicationUser user, string password);
    Task UpdateAsync(ApplicationUser user);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
}
