using CommonItems.Models;
using Microsoft.Extensions.Logging;

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
    private readonly IPickupTemporaryStorageService _pickupTemporaryStorageService;
    private readonly IPlaceInventoryStorageService _placeInventoryStorageService;

    public ReturnInventoryPalletService(
        ILogger<ReturnInventoryPalletService> logger,
        ITemporaryStorageLoader temporaryStorageLoader,
        IInventoryStorageLoader inventoryStorageLoader,
        IInventoryPalletLoader inventoryPalletLoader,
        IPickupTemporaryStorageService pickupTemporaryStorageService,
        IPlaceInventoryStorageService placeInventoryStorageService
    ) {
        _logger = logger;
        _temporaryStorageLoader = temporaryStorageLoader;
        _inventoryPalletLoader = inventoryPalletLoader;
        _inventoryStorageLoader = inventoryStorageLoader;
        _pickupTemporaryStorageService = pickupTemporaryStorageService;
        _placeInventoryStorageService = placeInventoryStorageService;
    }

    public void Request(LocationCode locationCode) {
        var temporaryStorageInfo = _temporaryStorageLoader.Find(locationCode);
        if (temporaryStorageInfo == null) {
            _logger.LogError($"一時置き場 [{locationCode.Value}] が見つかりませんでした");
            return;
        }
        if (temporaryStorageInfo.Status != TemporaryStorageStatus.InUse) {
            _logger.LogError($"一時置き場 [{locationCode.Value}] は使用中ではありません");
            return;
        }
        var inventoryPallet = _inventoryPalletLoader.Find(temporaryStorageInfo.InventoryPalletID);
        if (inventoryPallet == null) {
            _logger.LogError($"在庫パレット [{temporaryStorageInfo.InventoryPalletID.Value}] が見つかりませんでした");
            return;
        }
        var emptyLocation = _inventoryStorageLoader.FindEmptyLocation();
        if (emptyLocation == null) {
            _logger.LogError("空きロケーションが見つかりませんでした");
            return;
        }
        // 実際はここでAMRへの搬送指示を出し、搬送開始でPickup・搬送完了でPlaceを行う
        // 今回は搬送回数の試算のため搬送は瞬時に終わり失敗しない想定で進めるため Pickupと Placeを直接呼び出す
        _pickupTemporaryStorageService.Pickup(locationCode, temporaryStorageInfo.InventoryPalletID);
        _placeInventoryStorageService.Place(emptyLocation, temporaryStorageInfo.InventoryPalletID);
    }
}
