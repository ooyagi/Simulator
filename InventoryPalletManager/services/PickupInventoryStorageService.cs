using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;

namespace InventoryPalletCoordinator.Services;

/// <summary>
/// 在庫パレット置き場から在庫パレットを取り出すサービス
/// </summary>
class PickupInventoryStorageService: IPickupInventoryStorageService
{
    private readonly ILogger<PickupInventoryStorageService> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;
    private readonly IInventoryStorageLoader _inventoryStorageLoader;

    public PickupInventoryStorageService(
        ILogger<PickupInventoryStorageService> logger,
        IInventoryPalletCoordinatorDbContext context,
        IInventoryStorageLoader inventoryStorageLoader
    ) {
        _logger = logger;
        _context = context;
        _inventoryStorageLoader = inventoryStorageLoader;
    }

    public InventoryPalletID? Pickup(LocationCode locationCode) {
        var inventoryStorage = _inventoryStorageLoader.Find(locationCode);
        if (inventoryStorage == null) {
            _logger.LogError($"在庫パレット置き場 [{locationCode.Value}] が見つかりませんでした");
            return null;
        }
        var inventoryPalletID = inventoryStorage.Pickup();
        _context.SaveChanges();
        return inventoryPalletID;
    }
}
