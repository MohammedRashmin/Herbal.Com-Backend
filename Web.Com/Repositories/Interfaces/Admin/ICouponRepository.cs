using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.Admin;

public interface ICouponRepository
{
    Task<IEnumerable<Coupon>> GetAllAsync();
    Task<Coupon?> GetByIdAsync(Guid id);
    Task<Coupon?> GetByCodeAsync(string code);
    Task AddAsync(Coupon coupon);
    Task UpdateAsync(Coupon coupon);
    Task DeleteAsync(Coupon coupon);
}
