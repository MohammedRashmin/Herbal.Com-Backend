using Web.Com.DTOs.User;

namespace Web.Com.Services.Interfaces.User;

public interface IAddressService
{
    Task<List<AddressResponseDto>> GetAddressesAsync(string userId);
    Task<AddressResponseDto> CreateAddressAsync(string userId, CreateAddressDto dto);
    Task<bool> UpdateAddressAsync(string userId, Guid id, UpdateAddressDto dto);
    Task<bool> DeleteAddressAsync(string userId, Guid id);
    Task<bool> SetDefaultAddressAsync(string userId, Guid id);
}
