using Web.Com.Entities;

namespace Web.Com.Services.Interfaces.Shared;

public interface IShipStationService
{
    Task<string?> PushOrderAsync(Order order);
}
