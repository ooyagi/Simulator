using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class PutonShippingPalletItemService: ShippingOperationCoordinator.Interfaces.IPutonShippingPalletItemService
{
    private readonly ILogger<PutonShippingPalletItemService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly IShippingPalletLoader _shippingPalletLoader;

    public PutonShippingPalletItemService(
        ILogger<PutonShippingPalletItemService> logger,
        IShippingPalletCoordinatorDbContext context,
        IShippingStorageLoader shippingStorageLoader,
        IShippingPalletLoader shippingPalletLoader
    ) {
        _logger = logger;
        _context = context;
        _shippingStorageLoader = shippingStorageLoader;
        _shippingPalletLoader = shippingPalletLoader;
    }

    public void Puton(LocationCode locationCode, Hinban hinban) {
        _logger.LogInformation($"出荷パレット置き場 [{locationCode}] に品番 [{hinban}] を積載します");
        var shippingStorageInfo = _shippingStorageLoader.Find(locationCode);
        if (shippingStorageInfo == null) {
            _logger.LogError($"出荷パレット置き場 [{locationCode.Value}] が見つかりませんでした");
            return;
        }
        if (shippingStorageInfo.Status == StorageStatus.Empty || shippingStorageInfo.ShippingPalletID == null) {
            _logger.LogError($"出荷パレット置き場 [{locationCode.Value}] は使用されていません");
            return;
        }
        var pallet = _shippingPalletLoader.Find(shippingStorageInfo.ShippingPalletID);
        if (pallet == null) {
            _logger.LogError($"出荷パレット [{shippingStorageInfo.ShippingPalletID.Value}] が見つかりませんでした");
            return;
        }
        // 本番ではここで製品のシリアルコードを登録する
        pallet.PutonItem(hinban);
        _context.SaveChanges();
        return;
    }
}
