using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;

namespace ShippingPalletCoordinator.Services;

/// <summary>
/// 仕掛パレット置き場から仕掛パレットを取り出すサービス
/// </summary>
class PickupShikakariStorageService: IPickupShikakariStorageService
{
    private readonly ILogger<PickupShikakariStorageService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;
    private readonly IShikakariStorageLoader _shikakariStorageLoader;

    public PickupShikakariStorageService(
        ILogger<PickupShikakariStorageService> logger,
        IShippingPalletCoordinatorDbContext context,
        IShikakariStorageLoader shikakariStorageLoader
    ) {
        _logger = logger;
        _context = context;
        _shikakariStorageLoader = shikakariStorageLoader;
    }

    public ShippingPalletID? Pickup(LocationCode locationCode) {
        var shikakariStorage = _shikakariStorageLoader.Find(locationCode);
        if (shikakariStorage == null) {
            _logger.LogError($"仕掛パレット置き場 [{locationCode.Value}] が見つかりませんでした");
            return null;
        }
        var shikakariPalletID = shikakariStorage.Pickup();
        _context.SaveChanges();
        return shikakariPalletID;
    }
}
