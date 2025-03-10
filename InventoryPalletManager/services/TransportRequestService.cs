using Microsoft.Extensions.Logging;
using CommonItems.Interfaces;
using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

class TransportRequestService: ITransportRequestService
{
    private readonly ILogger<TransportRequestService> _logger;
    private readonly IPickupTemporaryStorageService _pickupTemporaryStorageService;
    private readonly IPickupInventoryStorageService _pickupInventoryStorageService;
    private readonly IPlaceTemporaryStorageService _placeTemporaryStorageService;
    private readonly IPlaceInventoryStorageService _placeInventoryStorageService;
    private readonly ITransportRecordRegister _transportRecordRegister;

    public TransportRequestService(
        ILogger<TransportRequestService> logger,
        IPickupTemporaryStorageService pickupTemporaryStorageService,
        IPickupInventoryStorageService pickupInventoryStorageService,
        IPlaceTemporaryStorageService placeTemporaryStorageService,
        IPlaceInventoryStorageService placeInventoryStorageService,
        ITransportRecordRegister transportRecordRegister
    ) {
        _logger = logger;
        _pickupTemporaryStorageService = pickupTemporaryStorageService;
        _pickupInventoryStorageService = pickupInventoryStorageService;
        _placeTemporaryStorageService = placeTemporaryStorageService;
        _placeInventoryStorageService = placeInventoryStorageService;
        _transportRecordRegister = transportRecordRegister;
    }

    /// <summary>
    /// 在庫パレットの搬送を依頼する
    /// 
    /// 実際はここでAMRへの搬送指示を出し、搬送開始でPickup・搬送完了でPlaceを行う
    /// 今回は搬送回数の試算のため搬送は瞬時に終わり失敗しない想定で進めるため Pickupと Placeを直接呼び出す
    /// </summary>
    public void Request(TransportType transportType, LocationCode from, LocationCode to) {
        if (transportType == TransportType.ReturnInventoryPallet) {
            TemporaryToInventory(from, to);
        } else if (transportType == TransportType.TakeInventoryPallet) {
            InventoryToTemporary(from, to);
        } else {
            throw new System.Exception("未対応の搬送種別です");
        }
    }
    private void TemporaryToInventory(LocationCode from, LocationCode to) {
        _logger.LogInformation($"在庫パレット返却要求を受け付けました: 一時置き場 [{from.Value}] 在庫ロケーション [{to.Value}]");
        var palletId = _pickupTemporaryStorageService.Pickup(from);
        if (palletId == null) {
            throw new InvalidOperationException($"一時置き場 [{from.Value}] からのパレット取り出しに失敗しました");
        }
        _placeInventoryStorageService.Place(to, palletId);
        _transportRecordRegister.Register(TransportType.ReturnInventoryPallet, from, to);
    }
    private void InventoryToTemporary(LocationCode from, LocationCode to) {
        _logger.LogInformation($"在庫パレット取り寄せ要求を受け付けました: 在庫ロケーション [{from.Value}] 一時置き場 [{to.Value}]");
        var palletId = _pickupInventoryStorageService.Pickup(from);
        if (palletId == null) {
            throw new InvalidOperationException($"在庫パレット置き場 [{from.Value}] からのパレット取り出しに失敗しました");
        }
        _placeTemporaryStorageService.Place(to, palletId);
        _transportRecordRegister.Register(TransportType.TakeInventoryPallet, from, to);
    }
}
