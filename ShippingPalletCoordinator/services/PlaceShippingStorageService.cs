using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;

namespace ShippingPalletCoordinator.Services;

class PlaceShippingStorageService: IPlaceShippingStorageService
{
    private readonly ILogger<PlaceShippingStorageService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;
    private readonly IShippingStorageLoader _shippingStorageLoader;

    public PlaceShippingStorageService(
        ILogger<PlaceShippingStorageService> logger,
        IShippingPalletCoordinatorDbContext context,
        IShippingStorageLoader shippingStorageLoader
    ) {
        _logger = logger;
        _context = context;
        _shippingStorageLoader = shippingStorageLoader;
    }

    public void Place(LocationCode locationCode, ShippingPalletID inventoryPalletID) {
        _logger.LogTrace($"{locationCode.Value} にパレット [{inventoryPalletID.Value}] を配置します");
        var shippingStorage = _shippingStorageLoader.Find(locationCode);
        if (shippingStorage == null) {
            throw new InvalidOperationException($"出荷パレット置き場 [{locationCode.Value}] が見つかりませんでした");
        }
        // if (shippingStorage.Status != StorageStatus.Empty) {
        //     throw new InvalidOperationException($"出荷パレット置き場 [{locationCode.Value}] は空きではありません");
        // }
        shippingStorage.Place(inventoryPalletID);
        _context.SaveChanges();
    }
}
