using Web.Com.DTOs.User;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.User;
using Web.Com.Services.Interfaces.User;

namespace Web.Com.Services.Implementations.User;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<List<AddressResponseDto>> GetAddressesAsync(string userId)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(userId);
        return addresses.Select(a => new AddressResponseDto
        {
            Id = a.Id,
            Street = a.Street,
            City = a.City,
            State = a.State,
            ZipCode = a.ZipCode,
            Country = a.Country,
            IsDefault = a.IsDefault
        }).ToList();
    }

    public async Task<AddressResponseDto> CreateAddressAsync(string userId, CreateAddressDto dto)
    {
        if (dto.IsDefault)
        {
            var defaults = await _addressRepository.GetDefaultsByUserIdAsync(userId);
            foreach (var a in defaults)
            {
                a.IsDefault = false;
                await _addressRepository.UpdateAsync(a);
            }
        }

        var hasAny = await _addressRepository.HasAnyAsync(userId);
        if (!hasAny) dto.IsDefault = true;

        var address = new Address
        {
            UserId = userId,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country,
            IsDefault = dto.IsDefault
        };

        await _addressRepository.AddAsync(address);

        return new AddressResponseDto
        {
            Id = address.Id,
            Street = address.Street,
            City = address.City,
            State = address.State,
            ZipCode = address.ZipCode,
            Country = address.Country,
            IsDefault = address.IsDefault
        };
    }

    public async Task<bool> UpdateAddressAsync(string userId, Guid id, UpdateAddressDto dto)
    {
        var address = await _addressRepository.GetByIdAsync(id, userId);
        if (address == null) return false;

        if (dto.IsDefault && !address.IsDefault)
        {
            var defaults = await _addressRepository.GetDefaultsByUserIdAsync(userId);
            foreach (var a in defaults)
            {
                if (a.Id != id)
                {
                    a.IsDefault = false;
                    await _addressRepository.UpdateAsync(a);
                }
            }
        }

        address.Street = dto.Street;
        address.City = dto.City;
        address.State = dto.State;
        address.ZipCode = dto.ZipCode;
        address.Country = dto.Country;
        address.IsDefault = dto.IsDefault;

        await _addressRepository.UpdateAsync(address);
        return true;
    }

    public async Task<bool> DeleteAddressAsync(string userId, Guid id)
    {
        var address = await _addressRepository.GetByIdAsync(id, userId);
        if (address == null) return false;

        await _addressRepository.DeleteAsync(address);

        if (address.IsDefault)
        {
            var next = await _addressRepository.GetMostRecentByUserIdAsync(userId);
            if (next != null)
            {
                next.IsDefault = true;
                await _addressRepository.UpdateAsync(next);
            }
        }

        return true;
    }

    public async Task<bool> SetDefaultAddressAsync(string userId, Guid id)
    {
        var address = await _addressRepository.GetByIdAsync(id, userId);
        if (address == null) return false;

        var defaults = await _addressRepository.GetDefaultsByUserIdAsync(userId);
        foreach (var a in defaults)
        {
            a.IsDefault = false;
            await _addressRepository.UpdateAsync(a);
        }

        address.IsDefault = true;
        await _addressRepository.UpdateAsync(address);
        return true;
    }
}
