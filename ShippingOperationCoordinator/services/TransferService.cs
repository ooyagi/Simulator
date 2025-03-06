using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class TransferService: ITransferService
{
    private readonly ILogger<TransferService> _logger;
    private readonly IDetermineTransferItemService _determineTransferItemService;
    private readonly IPickupInventoryPalletItemService _pickupInventoryPalletItemService;
    private readonly IPutonShippingPalletItemService _putonShippingPalletItemService;

    public TransferService(
        ILogger<TransferService> logger,
        IDetermineTransferItemService determineTransferItemService,
        IPickupInventoryPalletItemService pickupInventoryPalletItemService,
        IPutonShippingPalletItemService putonShippingPalletItemService)
    {
        _logger = logger;
        _determineTransferItemService = determineTransferItemService;
        _pickupInventoryPalletItemService = pickupInventoryPalletItemService;
        _putonShippingPalletItemService = putonShippingPalletItemService;
    }

    public bool ExecuteTransfer(ShippingStationCode stationCode) {
        try {
            _logger.LogInformation($"積替え処理開始： 出荷作業場所 [{stationCode}]");
            var transferDirection = _determineTransferItemService.DetermineTransferHinban(stationCode);
            if (transferDirection == null) {
                _logger.LogWarning("積替え対象が見つかりませんでした");
                return false;
            }
            var pickupResult = _pickupInventoryPalletItemService.Pickup(transferDirection.From);
            if (pickupResult == null) {
                _logger.LogWarning("在庫パレットからのピックアップに失敗しました");
                return false;
            }
            _putonShippingPalletItemService.Puton(transferDirection.To, pickupResult.Hinban);
            return true;
        } catch (Exception ex) {
            _logger.LogError(ex, "積替え処理中にエラーが発生しました");
            return false;
        }
    }
}
