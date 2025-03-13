using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryPalletCoordinator.Services;

class InventoryStorageLoader: IInventoryStorageLoader, ShippingOperationCoordinator.Interfaces.IInventoryStorageLoader
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

    public bool IsExists(Hinban hinban) {
        return _context.InventoryStorages.Include(x => x.StoredPallet).Any(x => x.StoredPallet.Hinban == hinban);
    }
    public InventoryStorage? Find(LocationCode locationCode) {
        return _context.InventoryStorages.FirstOrDefault(x => x.LocationCode == locationCode);
    }
    public InventoryStorage? FindEmptyLocation() {
        return _context.InventoryStorages
            .Where(x => x.Status == StorageStatus.Empty)
            .FirstOrDefault();
    }
    public LocationCode? FindStoredLocation(InventoryPalletID inventoryPalletID) {
        return _context.InventoryStorages
            .Where(x => x.InventoryPalletID == inventoryPalletID)
            .FirstOrDefault()?
            .LocationCode;
    }
    public int GetLastIndex() {
        return _context.InventoryStorages
            .OrderByDescending(x => x.Index)
            .FirstOrDefault()?
            .Index ?? 0;
    }

    public bool IsUseup(Hinban hinban, int loadableCount) {
        return _context.InventoryStorages
            .Include(x => x.StoredPallet)
            .Any(x => x.StoredPallet.Hinban == hinban
                && 0 < x.StoredPallet.Quantity
                && x.StoredPallet.Quantity <= loadableCount);
    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> GetStoragedItems() {
        return _context.InventoryStorages
            .Include(x => x.StoredPallet)
            .Where(x => 0 < x.StoredPallet.Quantity)
            .Select(x => new InventoryPalletInfo(x.LocationCode, x.StoredPallet.Hinban, x.StoredPallet.Quantity))
            .ToList();
    }

    record InventoryPalletInfo(LocationCode LocationCode, Hinban Hinban, int Quantity): ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo;
}
