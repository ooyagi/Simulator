using Microsoft.Extensions.Logging;
using CommonItems.Models;
using CommonItems.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

/// <summary>
/// 在庫パレット返却サービス
/// </summary>
class TakeInventoryPalletService: ShippingOperationCoordinator.Interfaces.ITakeInventoryPalletService
{
    private readonly ILogger<ReturnInventoryPalletService> _logger;
    private readonly ITemporaryStorageLoader _temporaryStorageLoader;
    private readonly IInventoryStorageLoader _inventoryStorageLoader;
    private readonly IInventoryPalletLoader _inventoryPalletLoader;
    private readonly IInboundInventoryPalletServices _inboundInventoryPalletServices;
    private readonly ITransportRequestService _transportRequestService;

    public TakeInventoryPalletService(
        ILogger<ReturnInventoryPalletService> logger,
        ITemporaryStorageLoader temporaryStorageLoader,
        IInventoryStorageLoader inventoryStorageLoader,
        IInventoryPalletLoader inventoryPalletLoader,
        IInboundInventoryPalletServices inboundInventoryPalletServices,
        ITransportRequestService transportRequestService
    ) {
        _logger = logger;
        _temporaryStorageLoader = temporaryStorageLoader;
        _inventoryPalletLoader = inventoryPalletLoader;
        _inventoryStorageLoader = inventoryStorageLoader;
        _inboundInventoryPalletServices = inboundInventoryPalletServices;
        _transportRequestService = transportRequestService;
    }

    /// <summary>
    /// 搬送数試算用の在庫パレット取り寄せサービス
    /// 
    /// 本番システムでは指定された品番の在庫パレットがない場合はエラーとなる
    /// 今回は搬送回数の試算が目的のため自動的に在庫は補充されるものとし、パレットを作成する
    /// 
    /// 本番システムでは搬送先、搬送元のロケーションをロックしてAMRへの搬送指示を出す
    /// AMRからの通知を受け、搬送開始でPickup・搬送完了でPlaceを行う
    /// 今回は搬送回数の試算のため搬送は瞬時に終わり失敗しない想定で進めるため Pickupと Placeを直接呼び出す
    /// </summary>
    public void Request(LocationCode tempLocationCode, Hinban hinban) {
        _logger.LogInformation($"在庫パレット取り寄せ要求を受け付けました: 一時置き場 [{tempLocationCode.Value}] 品番 [{hinban.Value}]");
        try {
            var inventoryPallet = _inventoryPalletLoader.FliterByHinban(hinban).Where(x => 0 < x.Quantity).OrderBy(x => x.Quantity).FirstOrDefault() ?? _inboundInventoryPalletServices.Inbound(hinban);
            var sourceLocationCode = _inventoryStorageLoader.FindStoredLocation(inventoryPallet.Id);
            if (sourceLocationCode == null) {
                _logger.LogError($"在庫パレット [{inventoryPallet.Id.Value}] が保管されているロケーションが見つかりませんでした");
                return;
            }
            var temporaryStorageInfo = _temporaryStorageLoader.Find(tempLocationCode);
            if (temporaryStorageInfo == null) {
                _logger.LogError($"一時置き場 [{tempLocationCode.Value}] が見つかりませんでした");
                return;
            }
            if (temporaryStorageInfo.Status != StorageStatus.Empty) {
                _logger.LogError($"一時置き場 [{tempLocationCode.Value}] は使用中です");
                return;
            }
            _transportRequestService.Request(TransportType.TakeInventoryPallet, sourceLocationCode, tempLocationCode);
        } catch (Exception ex) {
            _logger.LogError(ex, "在庫パレット取り寄せ処理中にエラーが発生しました");
        }
    }
}
