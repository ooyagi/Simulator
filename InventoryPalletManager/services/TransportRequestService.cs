using CommonItems.Interfaces;
using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

class TransportRequestService: ITransportRequestService
{
    private readonly IPickupTemporaryStorageService _pickupTemporaryStorageService;
    private readonly IPlaceInventoryStorageService _placeInventoryStorageService;
    private readonly ITransportRecordRegister _transportRecordRegister;

    public TransportRequestService(
        IPickupTemporaryStorageService pickupTemporaryStorageService,
        IPlaceInventoryStorageService placeInventoryStorageService,
        ITransportRecordRegister transportRecordRegister
    ) {
        _pickupTemporaryStorageService = pickupTemporaryStorageService;
        _placeInventoryStorageService = placeInventoryStorageService;
        _transportRecordRegister = transportRecordRegister;
    }

    /// <summary>
    /// 在庫パレットの搬送を依頼する
    /// 
    /// 実際はここでAMRへの搬送指示を出し、搬送開始でPickup・搬送完了でPlaceを行う
    /// 今回は搬送回数の試算のため搬送は瞬時に終わり失敗しない想定で進めるため Pickupと Placeを直接呼び出す
    /// </summary>
    public void Request(TransportType transportType, LocationCode from, LocationCode to, InventoryPalletID inventoryPalletID) {
        _pickupTemporaryStorageService.Pickup(from, inventoryPalletID);
        _placeInventoryStorageService.Place(to, inventoryPalletID);
        _transportRecordRegister.Register(transportType, from, to);
    }
}
