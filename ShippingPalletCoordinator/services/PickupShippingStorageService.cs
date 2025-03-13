using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;

namespace ShippingPalletCoordinator.Services;

/// <summary>
/// 出荷パレット置き場から出荷パレットを取り出すサービス
/// 
/// 出荷パレット置き場の出荷パレットから在庫を取り出すサービスではないので注意
/// </summary>
class PickupShippingStorageService: IPickupShippingStorageService
{
    private readonly ILogger<PickupShippingStorageService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly IShippingStorageEventPublisher _eventPublisher;

    public PickupShippingStorageService(
        ILogger<PickupShippingStorageService> logger,
        IShippingPalletCoordinatorDbContext context,
        IShippingStorageLoader shippingStorageLoader,
        IShippingStorageEventPublisher eventPublisher
    ) {
        _logger = logger;
        _context = context;
        _shippingStorageLoader = shippingStorageLoader;
        _eventPublisher = eventPublisher;
    }

    public ShippingPalletID? Pickup(LocationCode locationCode) {
        _logger.LogTrace($"{locationCode.Value} からパレットを取り出します");
        var shippingStorage = _shippingStorageLoader.Find(locationCode);
        if (shippingStorage == null) {
            _logger.LogError($"出荷パレット置き場 [{locationCode.Value}] が見つかりませんでした");
            return null;
        }
        var inventoryPalletID = shippingStorage.Pickup();
        _context.SaveChanges();
        _eventPublisher.PublishPickupEvent(locationCode);
        return inventoryPalletID;
    }
}
