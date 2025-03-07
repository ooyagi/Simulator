using Microsoft.Extensions.Logging;
using CommonItems.Models;
using CommonItems.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

/// <summary>
/// 在庫パレット返却サービス
/// </summary>
class ReturnInventoryPalletService: ShippingOperationCoordinator.Interfaces.IReturnInventoryPalletService
{
    private readonly ILogger<ReturnInventoryPalletService> _logger;
    private readonly ITemporaryStorageLoader _temporaryStorageLoader;
    private readonly IInventoryStorageLoader _inventoryStorageLoader;
    private readonly IInventoryPalletLoader _inventoryPalletLoader;
    private readonly ITransportRequestService _transportRequestService;

    public ReturnInventoryPalletService(
        ILogger<ReturnInventoryPalletService> logger,
        ITemporaryStorageLoader temporaryStorageLoader,
        IInventoryStorageLoader inventoryStorageLoader,
        IInventoryPalletLoader inventoryPalletLoader,
        ITransportRequestService transportRequestService
    ) {
        _logger = logger;
        _temporaryStorageLoader = temporaryStorageLoader;
        _inventoryPalletLoader = inventoryPalletLoader;
        _inventoryStorageLoader = inventoryStorageLoader;
        _transportRequestService = transportRequestService;
    }

    /// <summary>
    /// 搬送数試算用の在庫パレット返却サービス
    /// 
    /// 本番システムでは搬送先、搬送元のロケーションをロックしてAMRへの搬送指示を出す
    /// AMRからの通知を受け、搬送開始でPickup・搬送完了でPlaceを行う
    /// 今回は搬送回数の試算のため搬送は瞬時に終わり失敗しない想定で進めるため Pickupと Placeを直接呼び出す
    /// </summary>
    /// <param name="tempLocationCode"></param>
    public void Request(LocationCode tempLocationCode) {
        _logger.LogInformation($"在庫パレット返却要求を受け付けました: 一時置き場 [{tempLocationCode.Value}]");
        try {
            var temporaryStorageInfo = _temporaryStorageLoader.Find(tempLocationCode);
            if (temporaryStorageInfo == null) {
                _logger.LogError($"一時置き場 [{tempLocationCode.Value}] が見つかりませんでした");
                return;
            }
            if (temporaryStorageInfo.Status != StorageStatus.InUse || temporaryStorageInfo.InventoryPalletID == null) {
                _logger.LogError($"一時置き場 [{tempLocationCode.Value}] は使用中ではありません");
                return;
            }
            var inventoryPallet = _inventoryPalletLoader.Find(temporaryStorageInfo.InventoryPalletID);
            if (inventoryPallet == null) {
                _logger.LogError($"在庫パレット [{temporaryStorageInfo.InventoryPalletID.Value}] が見つかりませんでした");
                return;
            }
            var emptyLocationCode = _inventoryStorageLoader.FindEmptyLocation();
            if (emptyLocationCode == null) {
                _logger.LogError("空きロケーションが見つかりませんでした");
                return;
            }
            _transportRequestService.Request(TransportType.ReturnInventoryPallet, tempLocationCode, emptyLocationCode, temporaryStorageInfo.InventoryPalletID);
        } catch (Exception ex) {
            _logger.LogError(ex, "在庫パレット返却処理中にエラーが発生しました");
        }
    }
}
