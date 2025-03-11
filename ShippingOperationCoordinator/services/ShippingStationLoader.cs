using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ShippingStationLoader: IShippingStationLoader
{
    private readonly IShippingOperationCoordinatorDbContext _context;

    public ShippingStationLoader(
        IShippingOperationCoordinatorDbContext context
    ) {
        _context = context;
    }

    public IEnumerable<IShippingStation> All() {
        return _context.ShippingStations.Select(x => new ShippingStationResult(x.Code)).ToList();
    }
    record ShippingStationResult(ShippingStationCode Code): IShippingStation;
}
