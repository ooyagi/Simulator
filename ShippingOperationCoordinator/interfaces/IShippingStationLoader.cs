using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IShippingStationLoader
{
    IEnumerable<IShippingStation> All();
}
public interface IShippingStation
{
    ShippingStationCode Code { get; }
}
