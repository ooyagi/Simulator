using Microsoft.EntityFrameworkCore;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class ShikakariStorageLoader: Services.IShikakariStorageLoader, ShippingOperationCoordinator.Interfaces.IShikakariStorageLoader
{
    private readonly IShippingPalletCoordinatorDbContext _context;

    public ShikakariStorageLoader(
        IShippingPalletCoordinatorDbContext context
    ) {
        _context = context;
    }

    public ShikakariStorage? Find(LocationCode locationCode) {
        return _context.ShikakariStorages.FirstOrDefault(s => s.LocationCode == locationCode);
    }
    public LocationCode? FindStoredLocation(ShippingPalletID palletId) {
        var storage = _context.ShikakariStorages.FirstOrDefault(s => s.ShippingPalletID == palletId);
        return storage?.LocationCode;
    }
    public ShikakariStorage? FindEmptyLocation() {
        return _context.ShikakariStorages.FirstOrDefault(s => s.Status == StorageStatus.Empty);
    }
    public IEnumerable<ShikakariStorage> GetEmptyLocations() {
        return _context.ShikakariStorages.Where(s => s.Status == StorageStatus.Empty).ToList();
    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.ICompletablePalletInfo> FilterCompletableBy(IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> loadablePallets) {
        var shikakariStorages = _context.ShikakariStorages
            .Include(x => x.StoredPallet)
            .Where(x => x.Status == StorageStatus.InUse)
            .ToList();
        var loadableItems = loadablePallets.Select(x => new LoadableItem(x.Hinban, x.Quantity)).ToList();
        return shikakariStorages
            .Select(x => new { Pallet = x, Step = x.StoredPallet.GetStepToCompletion(loadableItems) })
            .Where(x => 0 < x.Step && x.Pallet.StoredPallet.NextHinban != null)
            .Select(x => new CompletablePalletInfo(x.Pallet.LocationCode, x.Pallet.ShippingPalletID!, x.Pallet.StoredPallet.NextHinban!, x.Step))
            .ToList();

    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.IShikakariPalletLoadableHinbanInfo> GetLoadableFrom(IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> loadablePallets) {
        var shippingStorages = _context.ShippingStorages
            .Include(x => x.StoredPallet)
            .Where(x => x.Status == StorageStatus.InUse)
            .ToList();
        var loadableItems = loadablePallets.Select(x => new LoadableItem(x.Hinban, x.Quantity)).ToList();
        return shippingStorages
            .Select(x => new ShippingPalletLoadableHinbanInfo(x.LocationCode, x.StoredPallet, loadableItems))
            .ToList();
    }

    record LoadableItem(Hinban Hinban, int Quantity): ILoadableItem;
    record CompletablePalletInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, int RemainStep): ShippingOperationCoordinator.Interfaces.ICompletablePalletInfo;
}
