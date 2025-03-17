using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryPalletCoordinator.Services;

class TemporaryStorageLoader: ITemporaryStorageLoader, ShippingOperationCoordinator.Interfaces.ITempStorageLoader
{
    private readonly ILogger<TemporaryStorageLoader> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;

    public TemporaryStorageLoader(
        ILogger<TemporaryStorageLoader> logger,
        IInventoryPalletCoordinatorDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public ShippingStationCode? ConvertStationCode(LocationCode locationCode) {
        return _context.TemporaryStorages
            .FirstOrDefault(x => x.LocationCode == locationCode)?
            .ShippingStationCode;
    }
    public bool IsPickable(ShippingStationCode stationCode, Hinban hinban, int quantity = 1) {
        return _context.TemporaryStorages
            .Include(x => x.StoredPallet)
            .Any(x => x.ShippingStationCode == stationCode
                && x.StoredPallet.Hinban == hinban
                && quantity <= x.StoredPallet.Quantity
            );
    }
    public bool InOtherStation(ShippingStationCode stationCode, Hinban hinban) {
        return _context.TemporaryStorages
            .Include(x => x.StoredPallet)
            .Any(x => x.ShippingStationCode != stationCode && x.StoredPallet.Hinban == hinban);
    }
    public IEnumerable<LocationCode> GetEmptyLocationCodes(ShippingStationCode stationCode) {
        return _context.TemporaryStorages
            .Where(x => x.ShippingStationCode == stationCode && x.Status == StorageStatus.Empty)
            .Select(x => x.LocationCode)
            .ToList();
    }
    public TemporaryStorage? Find(LocationCode locationCode) {
        return _context.TemporaryStorages
            .FirstOrDefault(x => x.LocationCode == locationCode);
    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> GetAvarableHinbans(ShippingStationCode stationCode) {
        return _context.TemporaryStorages
            .Include(x => x.StoredPallet)
            .Where(x => x.ShippingStationCode == stationCode && x.Status == StorageStatus.InUse && 0 < x.StoredPallet.Quantity)
            .Select(x => new InventoryPalletInfo(x.LocationCode, x.StoredPallet.Hinban, x.StoredPallet.Quantity))
            .ToList();
    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> GetAvarableHinbansInOtherStation(ShippingStationCode stationCode) {
        return _context.TemporaryStorages
            .Include(x => x.StoredPallet)
            .Where(x => x.ShippingStationCode != stationCode)
            .Select(x => new InventoryPalletInfo(x.LocationCode, x.StoredPallet.Hinban, x.StoredPallet.Quantity));
    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> GetEmptyPallets(ShippingStationCode stationCode) {
        return _context.TemporaryStorages
            .Include(x => x.StoredPallet)
            .Where(x => x.ShippingStationCode == stationCode && x.Status == StorageStatus.InUse && x.StoredPallet.Quantity == 0)
            .Select(x => new InventoryPalletInfo(x.LocationCode, x.StoredPallet.Hinban, x.StoredPallet.Quantity));
    }

    record InventoryPalletInfo(LocationCode LocationCode, Hinban Hinban, int Quantity): ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo;
}
