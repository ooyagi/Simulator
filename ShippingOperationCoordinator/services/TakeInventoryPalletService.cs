using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class TakeInventoryPalletService
{
    private readonly ILogger<TakeInventoryPalletService> _logger;
    private readonly ITempStorageLoader _tempStorageLoader;
    private readonly ITakeInventoryPalletSelector _takeInventoryPalletSelector;
    private readonly ITakeInventoryPalletService _takeInventoryPalletService;

    public TakeInventoryPalletService(
        ILogger<TakeInventoryPalletService> logger,
        ITempStorageLoader tempStorageLoader,
        ITakeInventoryPalletSelector takeInventoryPalletSelector,
        ITakeInventoryPalletService takeInventoryPalletService
    ) {
        _logger = logger;
        _tempStorageLoader = tempStorageLoader;
        _takeInventoryPalletSelector = takeInventoryPalletSelector;
        _takeInventoryPalletService = takeInventoryPalletService;
    }

    public void Take(ShippingStationCode stationCode) {
        _logger.LogInformation($"在庫パレット取り寄せ： 出荷作業場所[{stationCode}]");
        var emptyLocations = _tempStorageLoader.GetEmptyLocationCodes(stationCode);
        foreach (var emptyLocation in emptyLocations) {
            TakePallet(stationCode, emptyLocation);
        }
    }
    private void TakePallet(ShippingStationCode stationCode, LocationCode emptyLocation) {
        var hinban = _takeInventoryPalletSelector.SelectTakeInventoryPallet(stationCode);
        if (hinban == null) {
            return;
        }
        _takeInventoryPalletService.Request(emptyLocation, hinban);
    }
}
