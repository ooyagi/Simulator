using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class PlaceShikakariStorageService: IPlaceShikakariStorageService
{
    private readonly ILogger<PlaceShikakariStorageService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;
    private readonly IShikakariStorageLoader _shikakariStorageLoader;

    public PlaceShikakariStorageService(
        ILogger<PlaceShikakariStorageService> logger,
        IShippingPalletCoordinatorDbContext context,
        IShikakariStorageLoader shikakariStorageLoader
    ) {
        _logger = logger;
        _context = context;
        _shikakariStorageLoader = shikakariStorageLoader;
    }

    public void Place(LocationCode locationCode, ShippingPalletID shikakariPalletID) {
        _logger.LogTrace($"{locationCode.Value} にパレット [{shikakariPalletID.Value}] を配置します");
        var shikakariStorage = _shikakariStorageLoader.Find(locationCode);
        if (shikakariStorage == null) {
            throw new InvalidOperationException($"仕掛ロケーション [{locationCode.Value}] が見つかりませんでした");
        }
        if (shikakariStorage.Status != StorageStatus.Empty) {
            throw new InvalidOperationException($"仕掛ロケーション [{locationCode.Value}] は空きではありません: {shikakariStorage.Status} / {shikakariStorage.ShippingPalletID}");
        }
        shikakariStorage.Place(shikakariPalletID);
        shikakariStorage.Release();
        _context.SaveChanges();
    }
}
