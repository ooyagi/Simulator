using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class ShikakariStorageManagementService: IShikakariStorageManagementService
{
    private readonly ILogger<ShippingStorageManagementService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;

    public ShikakariStorageManagementService(
        ILogger<ShippingStorageManagementService> logger,
        IShippingPalletCoordinatorDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public ShikakariStorage Add(LocationCode locationCode) {
        var shikakariStorage = new ShikakariStorage(locationCode);
        _context.ShikakariStorages.Add(shikakariStorage);
        _context.SaveChanges();
        return shikakariStorage;
    }
    public void Clear() {
        var shikakariStorage = _context.ShikakariStorages;
        _context.ShikakariStorages.RemoveRange(shikakariStorage);
        _context.SaveChanges();
    }
}
