using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;

namespace InventoryPalletCoordinator.Services;

class PickupTemporaryStorageService: IPickupTemporaryStorageService
{
    private readonly ILogger<PickupTemporaryStorageService> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;
    private readonly ITemporaryStorageLoader _temporaryStorageLoader;
    private readonly ITemporaryStorageEventPublisher _eventPublisher;

    public PickupTemporaryStorageService(
        ILogger<PickupTemporaryStorageService> logger,
        IInventoryPalletCoordinatorDbContext context,
        ITemporaryStorageLoader temporaryStorageLoader,
        ITemporaryStorageEventPublisher eventPublisher
    ) {
        _logger = logger;
        _context = context;
        _temporaryStorageLoader = temporaryStorageLoader;
        _eventPublisher = eventPublisher;
    }

    public InventoryPalletID? Pickup(LocationCode locationCode) {
        var temporaryStorage = _temporaryStorageLoader.Find(locationCode);
        if (temporaryStorage == null) {
            _logger.LogError($"一時置き場 [{locationCode.Value}] が見つかりませんでした");
            return null;
        }
        var inventoryPalletID = temporaryStorage.Pickup();
        _context.SaveChanges();
        _eventPublisher.PublishPickupEvent(locationCode);
        return inventoryPalletID;
    }
}
