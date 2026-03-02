using Web.Com.DTOs.Admin;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Services.Implementations.Admin;

public class CouponService : ICouponService
{
    private readonly ICouponRepository _couponRepository;

    public CouponService(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<List<CouponResponseDto>> GetAllCouponsAsync()
    {
        var coupons = await _couponRepository.GetAllAsync();
        return coupons.Select(c => new CouponResponseDto
        {
            Id = c.Id,
            Code = c.Code,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            ExpiryDate = c.ExpiryDate,
            MinPurchase = c.MinPurchase,
            UsageLimit = c.UsageLimit,
            TimesUsed = c.TimesUsed,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<CouponResponseDto?> GetCouponByIdAsync(Guid id)
    {
        var c = await _couponRepository.GetByIdAsync(id);
        if (c == null) return null;

        return new CouponResponseDto
        {
            Id = c.Id,
            Code = c.Code,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            ExpiryDate = c.ExpiryDate,
            MinPurchase = c.MinPurchase,
            UsageLimit = c.UsageLimit,
            TimesUsed = c.TimesUsed,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        };
    }

    public async Task<CouponResponseDto> CreateCouponAsync(CreateCouponDto dto)
    {
        var coupon = new Coupon
        {
            Code = dto.Code.ToUpper().Trim(),
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            ExpiryDate = dto.ExpiryDate,
            MinPurchase = dto.MinPurchase,
            UsageLimit = dto.UsageLimit,
            IsActive = dto.IsActive
        };

        await _couponRepository.AddAsync(coupon);

        return new CouponResponseDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            ExpiryDate = coupon.ExpiryDate,
            MinPurchase = coupon.MinPurchase,
            UsageLimit = coupon.UsageLimit,
            TimesUsed = coupon.TimesUsed,
            IsActive = coupon.IsActive,
            CreatedAt = coupon.CreatedAt
        };
    }

    public async Task<bool> UpdateCouponAsync(Guid id, UpdateCouponDto dto)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null) return false;

        coupon.DiscountType = dto.DiscountType;
        coupon.DiscountValue = dto.DiscountValue;
        coupon.ExpiryDate = dto.ExpiryDate;
        coupon.MinPurchase = dto.MinPurchase;
        coupon.UsageLimit = dto.UsageLimit;
        coupon.IsActive = dto.IsActive;

        await _couponRepository.UpdateAsync(coupon);
        return true;
    }

    public async Task<bool> DeleteCouponAsync(Guid id)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null) return false;

        await _couponRepository.DeleteAsync(coupon);
        return true;
    }

    public async Task<ValidateCouponResponseDto> ValidateCouponAsync(string code, decimal cartTotal)
    {
        var coupon = await _couponRepository.GetByCodeAsync(code);

        if (coupon == null)
            return new ValidateCouponResponseDto { IsValid = false, Message = "Invalid coupon code" };

        if (!coupon.IsActive)
            return new ValidateCouponResponseDto { IsValid = false, Message = "Coupon is inactive" };

        if (DateTime.UtcNow > coupon.ExpiryDate)
            return new ValidateCouponResponseDto { IsValid = false, Message = "Coupon has expired" };

        if (coupon.UsageLimit > 0 && coupon.TimesUsed >= coupon.UsageLimit)
            return new ValidateCouponResponseDto { IsValid = false, Message = "Coupon usage limit reached" };

        if (cartTotal < coupon.MinPurchase)
            return new ValidateCouponResponseDto
            {
                IsValid = false,
                Message = $"Minimum purchase of ₹{coupon.MinPurchase:F2} required"
            };

        return new ValidateCouponResponseDto
        {
            IsValid = true,
            Message = "Coupon applied successfully",
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue
        };
    }
}
