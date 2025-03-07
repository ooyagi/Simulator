using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Models;
using InventoryPalletCoordinator.Interfaces;

namespace InventoryPalletCoordinator.Services;

class PickupInventoryPalletItemService: ShippingOperationCoordinator.Interfaces.IPickupInventoryPalletItemService
{
    private readonly ILogger<PickupInventoryPalletItemService> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;
    private readonly ITemporaryStorageLoader _temporaryStorageLoader;
    private readonly IInventoryPalletLoader _inventoryPalletLoader;

    public PickupInventoryPalletItemService(
        ILogger<PickupInventoryPalletItemService> logger,
        IInventoryPalletCoordinatorDbContext context,
        ITemporaryStorageLoader temporaryStorageLoader,
        IInventoryPalletLoader inventoryPalletLoader
    ) {
        _logger = logger;
        _context = context;
        _temporaryStorageLoader = temporaryStorageLoader;
        _inventoryPalletLoader = inventoryPalletLoader;
    }

    public ShippingOperationCoordinator.Interfaces.IPickupItemResult? Pickup(LocationCode locationCode) {
        var temporaryStorageInfo = _temporaryStorageLoader.Find(locationCode);
        if (temporaryStorageInfo == null) {
            _logger.LogError($"一時置き場 [{locationCode.Value}] が見つかりませんでした");
            return null;
        }
        if (temporaryStorageInfo.Status == StorageStatus.Empty || temporaryStorageInfo.InventoryPalletID == null) {
            _logger.LogError($"一時置き場 [{locationCode.Value}] は空です");
            return null;
        }
        var pallet = _inventoryPalletLoader.Find(temporaryStorageInfo.InventoryPalletID);
        if (pallet == null) {
            _logger.LogError($"在庫パレット [{temporaryStorageInfo.InventoryPalletID.Value}] が見つかりませんでした");
            return null;
        }
        // 本番ではここで製品のシリアルコードを取得する
        pallet.PickupItem();
        _context.SaveChanges();
        return new PickupItemResult(pallet.Hinban);
    }

    record PickupItemResult(Hinban Hinban): ShippingOperationCoordinator.Interfaces.IPickupItemResult;
}
