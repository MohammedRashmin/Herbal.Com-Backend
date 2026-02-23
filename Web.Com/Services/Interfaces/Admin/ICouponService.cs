using Web.Com.DTOs.Admin;

namespace Web.Com.Services.Interfaces.Admin;

public interface ICouponService
{
    Task<List<CouponResponseDto>> GetAllCouponsAsync();
    Task<CouponResponseDto?> GetCouponByIdAsync(int id);
    Task<CouponResponseDto> CreateCouponAsync(CreateCouponDto dto);
    Task<bool> UpdateCouponAsync(int id, UpdateCouponDto dto);
    Task<bool> DeleteCouponAsync(int id);
    Task<ValidateCouponResponseDto> ValidateCouponAsync(string code, decimal cartTotal);
}
