using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ShippingStationLoader: IShippingStationLoader
{
    public IEnumerable<IShippingStation> All() {
        return new List<IShippingStation>();
    }
}
