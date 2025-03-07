using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

class PlaceInventoryStorageService: IPlaceInventoryStorageService
{
    private readonly ILogger<PlaceInventoryStorageService> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;
    private readonly IInventoryStorageLoader _inventoryStorageLoader;

    public PlaceInventoryStorageService(
        ILogger<PlaceInventoryStorageService> logger,
        IInventoryPalletCoordinatorDbContext context,
        IInventoryStorageLoader inventoryStorageLoader
    ) {
        _logger = logger;
        _context = context;
        _inventoryStorageLoader = inventoryStorageLoader;
    }

    public void Place(LocationCode locationCode, InventoryPalletID inventoryPalletID) {
        var inventoryStorage = _inventoryStorageLoader.Find(locationCode);
        if (inventoryStorage == null) {
            throw new InvalidOperationException($"在庫ロケーション [{locationCode.Value}] が見つかりませんでした");
        }
        if (inventoryStorage.Status != StorageStatus.Empty) {
            throw new InvalidOperationException($"在庫ロケーション [{locationCode.Value}] は空きではありません");
        }
        inventoryStorage.Place(inventoryPalletID);
        _context.SaveChanges();
    }
}
