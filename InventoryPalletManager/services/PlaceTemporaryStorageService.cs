using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

class PlaceTemporaryStorageService: IPlaceTemporaryStorageService
{
    private readonly ILogger<PlaceTemporaryStorageService> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;
    private readonly ITemporaryStorageLoader _temporaryStorageLoader;

    public PlaceTemporaryStorageService(
        ILogger<PlaceTemporaryStorageService> logger,
        IInventoryPalletCoordinatorDbContext context,
        ITemporaryStorageLoader temporaryStorageLoader
    ) {
        _logger = logger;
        _context = context;
        _temporaryStorageLoader = temporaryStorageLoader;
    }

    public void Place(LocationCode locationCode, InventoryPalletID inventoryPalletID) {
        var temporaryStorage = _temporaryStorageLoader.Find(locationCode);
        if (temporaryStorage == null) {
            throw new InvalidOperationException($"一時置き場 [{locationCode.Value}] が見つかりませんでした");
        }
        if (temporaryStorage.Status != StorageStatus.Empty) {
            throw new InvalidOperationException($"一時置き場 [{locationCode.Value}] は空きではありません");
        }
        temporaryStorage.Place(inventoryPalletID);
        _context.SaveChanges();
    }
}
