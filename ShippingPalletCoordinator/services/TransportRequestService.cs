using Microsoft.Extensions.Logging;
using CommonItems.Interfaces;
using CommonItems.Models;

namespace ShippingPalletCoordinator.Services;

class TransportRequestService: ITransportRequestService
{
    private readonly ILogger<TransportRequestService> _logger;
    private readonly IPickupShippingStorageService _pickupShippingStorageService;
    private readonly IPickupShikakariStorageService _pickupShikakariStorageService;
    private readonly IPlaceShippingStorageService _placeShippingStorageService;
    private readonly IPlaceShikakariStorageService _placeShikakariStorageService;
    private readonly ITransportRecordRegister _transportRecordRegister;

    public TransportRequestService(
        ILogger<TransportRequestService> logger,
        IPickupShippingStorageService pickupShippingStorageService,
        IPickupShikakariStorageService pickupShikakariStorageService,
        IPlaceShippingStorageService placeShippingStorageService,
        IPlaceShikakariStorageService placeShikakariStorageService,
        ITransportRecordRegister transportRecordRegister
    ) {
        _logger = logger;
        _pickupShippingStorageService = pickupShippingStorageService;
        _pickupShikakariStorageService = pickupShikakariStorageService;
        _placeShippingStorageService = placeShippingStorageService;
        _placeShikakariStorageService = placeShikakariStorageService;
        _transportRecordRegister = transportRecordRegister;
    }

    /// <summary>
    /// 出荷パレットの搬送を依頼する
    /// 
    /// 実際はここでAMRへの搬送指示を出し、搬送開始でPickup・搬送完了でPlaceを行う
    /// 今回は搬送回数の試算のため搬送は瞬時に終わり失敗しない想定で進めるため Pickupと Placeを直接呼び出す
    /// </summary>
    public void Request(TransportType transportType, LocationCode from, LocationCode to) {
        if (transportType == TransportType.ReturnShippingPallet) {
            ShippingToShikakari(from, to);
        } else if (transportType == TransportType.TakeShippingPallet) {
            ShikakariToShipping(from, to);
        } else {
            throw new Exception($"未対応の搬送種別です: {transportType.Value}");
        }
    }
    private void ShippingToShikakari(LocationCode from, LocationCode to) {
        _logger.LogTrace($"パレット出荷パレット置き場 [{from.Value}] から仕掛パレット置き場 [{to.Value}] に搬送します");
        var palletId = _pickupShippingStorageService.Pickup(from);
        if (palletId == null) {
            throw new InvalidOperationException($"出荷パレット置き場 [{from.Value}] からのパレット取り出しに失敗しました");
        }
        _placeShikakariStorageService.Place(to, palletId);
        _transportRecordRegister.Register(TransportType.ReturnShippingPallet, from, to);
    }
    private void ShikakariToShipping(LocationCode from, LocationCode to) {
        _logger.LogTrace($"パレット仕掛パレット置き場 [{from.Value}] から出荷パレット置き場 [{to.Value}] に搬送します");
        var palletId = _pickupShikakariStorageService.Pickup(from);
        if (palletId == null) {
            throw new InvalidOperationException($"仕掛置き場 [{from.Value}] からのパレット取り出しに失敗しました");
        }
        _placeShippingStorageService.Place(to, palletId);
        _transportRecordRegister.Register(TransportType.TakeShippingPallet, from, to);
    }
}
