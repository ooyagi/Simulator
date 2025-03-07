using CommonItems.Interfaces;
using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

class TransportRequestService: ITransportRequestService
{
    private readonly IPickupTemporaryStorageService _pickupTemporaryStorageService;
    private readonly IPickupInventoryStorageService _pickupInventoryStorageService;
    private readonly IPlaceTemporaryStorageService _placeTemporaryStorageService;
    private readonly IPlaceInventoryStorageService _placeInventoryStorageService;
    private readonly ITransportRecordRegister _transportRecordRegister;

    public TransportRequestService(
        IPickupTemporaryStorageService pickupTemporaryStorageService,
        IPickupInventoryStorageService pickupInventoryStorageService,
        IPlaceTemporaryStorageService placeTemporaryStorageService,
        IPlaceInventoryStorageService placeInventoryStorageService,
        ITransportRecordRegister transportRecordRegister
    ) {
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
        var palletId = _pickupTemporaryStorageService.Pickup(from);
        _placeInventoryStorageService.Place(to, palletId);
        _transportRecordRegister.Register(TransportType.ReturnInventoryPallet, from, to);
    }
    private void InventoryToTemporary(LocationCode from, LocationCode to) {
        var palletId = _pickupInventoryStorageService.Pickup(from);
        _placeTemporaryStorageService.Place(to, palletId);
        _transportRecordRegister.Register(TransportType.TakeInventoryPallet, from, to);
    }
}
