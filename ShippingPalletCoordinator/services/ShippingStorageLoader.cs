using CommonItems.Models;
using Microsoft.EntityFrameworkCore;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class ShippingStorageLoader: Services.IShippingStorageLoader, ShippingOperationCoordinator.Interfaces.IShippingStorageLoader
{
    private readonly IShippingPalletCoordinatorDbContext _context;

    public ShippingStorageLoader(
        IShippingPalletCoordinatorDbContext context
    ) {
        _context = context;
    }

    public ShippingStationCode? ConvertStationCode(LocationCode locationCode) {
        return _context.ShippingStorages.FirstOrDefault(x => x.LocationCode == locationCode)?.ShippingStationCode;
    }
    public ShippingStorage? Find(LocationCode locationCode) {
        return _context.ShippingStorages.FirstOrDefault(x => x.LocationCode == locationCode);
    }
    public IEnumerable<LocationCode> GetEmptyLocationCodes(ShippingStationCode stationCode) {
        return _context.ShippingStorages
            .Where(x => x.ShippingStationCode == stationCode && x.Status == StorageStatus.Empty)
            .Select(x => x.LocationCode);
    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.IShippingPalletInfo> All(ShippingStationCode stationCode) {
        return _context.ShippingStorages
            .Include(x => x.StoredPallet)
            .Where(x => x.ShippingStationCode == stationCode && x.ShippingPalletID != null)
            .Select(x => new ShippingPalletInfo(x.LocationCode, x.ShippingPalletID, x.StoredPallet.NextHinban, x.StoredPallet.IsCompleted));
    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.ICompletablePalletInfo> FilterCompletableBy(ShippingStationCode stationCode, IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> loadablePallets) {
        var shippingStorages = _context.ShippingStorages
            .Include(x => x.StoredPallet)
            .Where(x => x.ShippingStationCode == stationCode && x.Status == StorageStatus.InUse)
            .ToList();
        var loadableItems = loadablePallets.Select(x => new LoadableItem(x.Hinban, x.Quantity)).ToList();
        return shippingStorages
            .Select(x => new { Pallet = x, Step = x.StoredPallet.GetStepToCompletion(loadableItems) })
            .Where(x => 0 < x.Step)
            .Select(x => new CompletablePalletInfo(x.Pallet.LocationCode, x.Pallet.ShippingPalletID!, x.Pallet.StoredPallet.NextHinban, x.Step))
            .ToList();
    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.IShippingPalletLoadableHinbanInfo> GetLoadableFrom(ShippingStationCode stationCode, IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> loadablePallets) {
        var shippingStorages = _context.ShippingStorages
            .Include(x => x.StoredPallet)
            .Where(x => x.ShippingStationCode == stationCode && x.Status == StorageStatus.InUse)
            .ToList();
        var loadableItems = loadablePallets.Select(x => new LoadableItem(x.Hinban, x.Quantity)).ToList();
        return shippingStorages
            .Select(x => new ShippingPalletLoadableHinbanInfo(x.LocationCode, x.StoredPallet))
            .ToList();
    }

    record LoadableItem(Hinban Hinban, int Quantity): ILoadableItem;
    record ShippingPalletInfo(LocationCode LocationCode, ShippingPalletID? ShippingPalletID, Hinban? NextHinban, bool IsCompleted): ShippingOperationCoordinator.Interfaces.IShippingPalletInfo;
    record CompletablePalletInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, int RemainStep): ShippingOperationCoordinator.Interfaces.ICompletablePalletInfo;
}
