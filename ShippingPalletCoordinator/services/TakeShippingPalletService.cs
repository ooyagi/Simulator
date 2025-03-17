using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

/// <summary>
/// 出荷パレット返却サービス
/// </summary>
class TakeShippingPalletService: ShippingOperationCoordinator.Interfaces.ITakeShippingPalletService
{
    private readonly ILogger<TakeShippingPalletService> _logger;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly IShikakariStorageLoader _shikakariStorageLoader;
    private readonly IShippingPalletLoader _shippingPalletLoader;
    private readonly ITransportRequestService _transportRequestService;

    public TakeShippingPalletService(
        ILogger<TakeShippingPalletService> logger,
        IShippingStorageLoader shippingStorageLoader,
        IShikakariStorageLoader shikakariStorageLoader,
        IShippingPalletLoader shippingPalletLoader,
        ITransportRequestService transportRequestService
    ) {
        _logger = logger;
        _shippingStorageLoader = shippingStorageLoader;
        _shikakariStorageLoader = shikakariStorageLoader;
        _shippingPalletLoader = shippingPalletLoader;
        _transportRequestService = transportRequestService;
    }

    /// <summary>
    /// 搬送数試算用の出荷パレット取り寄せサービス
    /// </summary>
    public void Request(LocationCode locationCode, ShippingPalletID palletId) {
        _logger.LogInformation($"出荷パレット取り寄せ要求を受け付けました: 出荷パレット置き場 [{locationCode.Value}] パレット [{palletId.Value}]");
        try {
            var shippingPallet = _shippingPalletLoader.Find(palletId);
            if (shippingPallet == null) {
                _logger.LogError($"出荷パレット [{palletId.Value}] が見つかりませんでした");
                return;
            }
            var sourceLocationCode = _shikakariStorageLoader.FindStoredLocation(shippingPallet.Id);
            if (sourceLocationCode == null) {
                _logger.LogError($"出荷パレット [{shippingPallet.Id.Value}] が保管されているロケーションが見つかりませんでした");
                return;
            }
            var shippingLocationInfo = _shippingStorageLoader.Find(locationCode);
            if (shippingLocationInfo == null) {
                _logger.LogError($"出荷パレット置き場 [{locationCode.Value}] が見つかりませんでした");
                return;
            }
            if (shippingLocationInfo.Status != StorageStatus.Empty) {
                _logger.LogError($"出荷パレット置き場 [{locationCode.Value}] は空きではありません");
                return;
            }
            _transportRequestService.Request(TransportType.TakeShippingPallet, sourceLocationCode, shippingLocationInfo.LocationCode);
        } catch (Exception ex) {
            _logger.LogError(ex, "出荷パレット返却処理中にエラーが発生しました");
        }
    }
}
