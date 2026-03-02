using Web.Com.DTOs.Admin;

namespace Web.Com.Services.Interfaces.Admin;

public interface ICouponService
{
    Task<List<CouponResponseDto>> GetAllCouponsAsync();
    Task<CouponResponseDto?> GetCouponByIdAsync(Guid id);
    Task<CouponResponseDto> CreateCouponAsync(CreateCouponDto dto);
    Task<bool> UpdateCouponAsync(Guid id, UpdateCouponDto dto);
    Task<bool> DeleteCouponAsync(Guid id);
    Task<ValidateCouponResponseDto> ValidateCouponAsync(string code, decimal cartTotal);
}
