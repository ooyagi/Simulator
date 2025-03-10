using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

/// <summary>
/// 出荷パレット返却サービス
/// </summary>
class ReturnShippingPalletService: ShippingOperationCoordinator.Interfaces.IReturnShippingPalletService
{
    private readonly ILogger<ReturnShippingPalletService> _logger;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly IShikakariStorageLoader _shikakariStorageLoader;
    private readonly IShippingPalletLoader _shippingPalletLoader;
    private readonly ITransportRequestService _transportRequestService;
    private readonly IRotateShippingPalletService _rotateShippingPalletService;

    public ReturnShippingPalletService(
        ILogger<ReturnShippingPalletService> logger,
        IShippingStorageLoader shippingStorageLoader,
        IShikakariStorageLoader shikakariStorageLoader,
        IShippingPalletLoader shippingPalletLoader,
        ITransportRequestService transportRequestService,
        IRotateShippingPalletService rotateShippingPalletService
    ) {
        _logger = logger;
        _shippingStorageLoader = shippingStorageLoader;
        _shikakariStorageLoader = shikakariStorageLoader;
        _shippingPalletLoader = shippingPalletLoader;
        _transportRequestService = transportRequestService;
        _rotateShippingPalletService = rotateShippingPalletService;
    }

    /// <summary>
    /// 搬送数試算用の出荷パレット返却サービス
    /// </summary>
    public void Request(LocationCode shippingLocationCode) {
        _logger.LogInformation($"出荷パレット返却要求を受け付けました: 出荷作業場所 [{shippingLocationCode.Value}]");
        try {
            var shippingStorageInfo = _shippingStorageLoader.Find(shippingLocationCode);
            if (shippingStorageInfo == null) {
                _logger.LogError($"出荷パレット置き場 [{shippingLocationCode.Value}] が見つかりませんでした");
                return;
            }
            if (shippingStorageInfo.Status != StorageStatus.InUse || shippingStorageInfo.ShippingPalletID == null) {
                _logger.LogError($"出荷パレット置き場 [{shippingLocationCode.Value}] は使用中ではありません");
                return;
            }
            var shippingPallet = _shippingPalletLoader.Find(shippingStorageInfo.ShippingPalletID);
            if (shippingPallet == null) {
                _logger.LogError($"出荷パレット [{shippingStorageInfo.ShippingPalletID.Value}] が見つかりませんでした");
                return;
            }
            var emptyLocationCode = _shikakariStorageLoader.FindEmptyLocation()?.LocationCode;
            if (emptyLocationCode == null) {
                _logger.LogError("空きロケーションが見つかりませんでした");
                return;
            }
            _transportRequestService.Request(TransportType.ReturnShippingPallet, shippingLocationCode, emptyLocationCode);
            // 返却した出荷パレットが完了していれば入れ替え
            if(shippingPallet.IsCompleted) {
                _rotateShippingPalletService.Rotate(shippingPallet.Id);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "出荷パレット返却処理中にエラーが発生しました");
        }
    }
}
