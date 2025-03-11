using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

class TemporaryStorageManagementService: ITemporaryStorageManagementService
{
    private readonly ILogger<TemporaryStorageManagementService> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;

    public TemporaryStorageManagementService(
        ILogger<TemporaryStorageManagementService> logger,
        IInventoryPalletCoordinatorDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public TemporaryStorage Add(ShippingStationCode shippingStationCode, LocationCode locationCode) {
        var temporaryStorage = new TemporaryStorage(shippingStationCode, locationCode);
        _context.TemporaryStorages.Add(temporaryStorage);
        _context.SaveChanges();
        return temporaryStorage;
    }
    public void Clear() {
        var temporaryStorage = _context.TemporaryStorages;
        _context.TemporaryStorages.RemoveRange(temporaryStorage);
        _context.SaveChanges();
    }
}
