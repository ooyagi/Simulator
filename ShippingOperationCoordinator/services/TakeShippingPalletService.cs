using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class TakeShippingPalletService
{
    private readonly ILogger<TakeShippingPalletService> _logger;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly ITakeShippingPalletSelector _takeShippingPalletSelector;
    private readonly ITakeShippingPalletService _takeShippingPalletService;

    public TakeShippingPalletService(
        ILogger<TakeShippingPalletService> logger,
        IShippingStorageLoader shippingStorageLoader,
        ITakeShippingPalletSelector takeShippingPalletSelector,
        ITakeShippingPalletService takeShippingPalletService
    ) {
        _logger = logger;
        _shippingStorageLoader = shippingStorageLoader;
        _takeShippingPalletSelector = takeShippingPalletSelector;
        _takeShippingPalletService = takeShippingPalletService;
    }

    public void Take(ShippingStationCode stationCode) {
        _logger.LogInformation($"出荷パレット取り寄せ： 出荷作業場所[{stationCode}]");
        var emptyLocations = _shippingStorageLoader.GetEmptyLocationCodes(stationCode);
        foreach (var emptyLocation in emptyLocations) {
            TakePallet(stationCode, emptyLocation);
        }
    }
    private void TakePallet(ShippingStationCode stationCode, LocationCode emptyLocation) {
        var palletId = _takeShippingPalletSelector.SelectTakeShippingPallet(stationCode);
        if (palletId == null) {
            return;
        }
        _takeShippingPalletService.Request(emptyLocation, palletId);
    }
}
