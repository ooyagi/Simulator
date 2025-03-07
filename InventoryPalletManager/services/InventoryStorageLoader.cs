using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

class InventoryStorageLoader: IInventoryStorageLoader
{
    private readonly ILogger<InventoryStorageLoader> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;

    public InventoryStorageLoader(
        ILogger<InventoryStorageLoader> logger,
        IInventoryPalletCoordinatorDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public InventoryStorage? Find(LocationCode locationCode) {
        return _context.InventoryStorages.FirstOrDefault(x => x.LocationCode == locationCode);
    }
    public LocationCode? FindEmptyLocation() {
        return _context.InventoryStorages
            .Where(x => x.Status == StorageStatus.Empty)
            .FirstOrDefault()?
            .LocationCode;
    }
}
