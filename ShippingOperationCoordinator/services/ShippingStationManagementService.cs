using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;
using ShippingOperationCoordinator.Models;

namespace ShippingOperationCoordinator.Services;

class ShippingStationManagementService: IShippingStationManagementService
{
    private readonly ILogger<ShippingStationManagementService> _logger;
    private readonly IShippingOperationCoordinatorDbContext _context;

    public ShippingStationManagementService(
        ILogger<ShippingStationManagementService> logger,
        IShippingOperationCoordinatorDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public ShippingStation Add(ShippingStationCode shippingStationCode) {
        var shippingStation = new ShippingStation(shippingStationCode);
        _context.ShippingStations.Add(shippingStation);
        _context.SaveChanges();
        return shippingStation;
    }
    public void Clear() {
        var shippingStation = _context.ShippingStations;
        _context.ShippingStations.RemoveRange(shippingStation);
        _context.SaveChanges();
    }
}
