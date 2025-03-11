using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class ShippingStorageManagementService: IShippingStorageManagementService
{
    private readonly ILogger<ShippingStorageManagementService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;
    private readonly IShippingStorageLoader _shippingStorageLoader;

    public ShippingStorageManagementService(
        ILogger<ShippingStorageManagementService> logger,
        IShippingPalletCoordinatorDbContext context,
        IShippingStorageLoader shippingStorageLoader
    ) {
        _logger = logger;
        _context = context;
        _shippingStorageLoader = shippingStorageLoader;
    }

    public ShippingStorage Add(ShippingStationCode shippingStationCode, LocationCode locationCode) {
        var lastIndex = _shippingStorageLoader.GetLastIndex(shippingStationCode);
        var shippingStorage = new ShippingStorage(shippingStationCode, locationCode, lastIndex + 1);
        _context.ShippingStorages.Add(shippingStorage);
        _context.SaveChanges();
        return shippingStorage;
    }
    public void Clear() {
        var shippingStorage = _context.ShippingStorages;
        _context.ShippingStorages.RemoveRange(shippingStorage);
        _context.SaveChanges();
    }
}
