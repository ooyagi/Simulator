using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class RotateShippingPalletService: IRotateShippingPalletService
{
    private readonly ILogger<RotateShippingPalletService> _logger;
    private readonly IWorkOrderLoader _workOrderLoader;
    private readonly IShikakariStorageLoader _shikakariStorageLoader;
    private readonly IInboundShippingPalletService _inboundShippingPalletService;
    private readonly IOutboundShippingPalletService _outboundShippingPalletService;
    private readonly IPickupShikakariStorageService _pickupShikakariStorageService;
    private readonly IPlaceShikakariStorageService _placeShikakariStorageService;

    public RotateShippingPalletService(
        ILogger<RotateShippingPalletService> logger,
        IWorkOrderLoader workOrderLoader,
        IShikakariStorageLoader shikakariStorageLoader,
        IInboundShippingPalletService inboundShippingPalletService,
        IOutboundShippingPalletService outboundShippingPalletService, 
        IPickupShikakariStorageService pickupShikakariStorageService,
        IPlaceShikakariStorageService placeShikakariStorageService
    ) {
        _logger = logger;
        _workOrderLoader = workOrderLoader;
        _shikakariStorageLoader = shikakariStorageLoader;
        _inboundShippingPalletService = inboundShippingPalletService;
        _outboundShippingPalletService = outboundShippingPalletService;
        _pickupShikakariStorageService = pickupShikakariStorageService;
        _placeShikakariStorageService = placeShikakariStorageService;
    }

    public void Rotate(ShippingPalletID shippingPalletID) {
        _logger.LogInformation($"出荷パレットを入れ替えます: 出荷パレット [{shippingPalletID.Value}]");
        try {
            var targetLocationCode = _shikakariStorageLoader.FindStoredLocation(shippingPalletID);
            if (targetLocationCode == null) {
                _logger.LogError($"出荷パレット [{shippingPalletID.Value}] が見つかりませんでした");
                return;
            }
            Outbound(targetLocationCode);
            InboundNextPallet(targetLocationCode);
        } catch (Exception ex) {
            _logger.LogError(ex, "出荷パレット入れ替え処理中にエラーが発生しました");
        }
    }
    public void InboundNextPallet(LocationCode targetLocationCode) {
        _logger.LogInformation($"次の出荷パレットを搬入します: 位置 [{targetLocationCode.Value}]");
        try {
            var nextOrder = _workOrderLoader.GetNextOrder();
            if (nextOrder == null) {
                // 最後は指示書が切れて終わるので異常ではない
                _logger.LogDebug("次の作業指示が見つかりませんでした");
                return;
            }
            var nextShippingPallet = new ShippingPallet(
                nextOrder.ShippingPalletID,
                nextOrder.Priority,
                nextOrder.WorkItems.Select(x => new ShippingPalletItem(nextOrder.ShippingPalletID, x.Hinban, x.Index)).ToList()
            );
            _inboundShippingPalletService.Inbound(nextShippingPallet);
            _placeShikakariStorageService.Place(targetLocationCode, nextShippingPallet.Id);
        } catch (Exception ex) {
            _logger.LogError(ex, "次の出荷パレット搬入処理中にエラーが発生しました");
        }
    }
    public void Outbound(LocationCode targetLocationCode) {
        _logger.LogInformation($"出荷パレットを出庫します: 位置 [{targetLocationCode.Value}]");
        try {
            var shippingPalletID = _pickupShikakariStorageService.Pickup(targetLocationCode);
            if (shippingPalletID == null) {
                _logger.LogError($"位置 [{targetLocationCode.Value}] に出荷パレットが見つかりませんでした");
                return;
            }
            _outboundShippingPalletService.Outbound(shippingPalletID);
        } catch (Exception ex) {
            _logger.LogError(ex, "出荷パレット出庫処理中にエラーが発生しました");
        }
    }
}
