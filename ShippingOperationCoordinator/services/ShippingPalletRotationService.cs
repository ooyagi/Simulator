using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ShippingPalletRotationService
{
    private readonly ILogger<ShippingPalletRotationService> _logger;
    private readonly IReturnShippingPalletSelector _returnShippingPalletSelector;
    private readonly IReturnShippingPalletService _returnShippingPalletService;

    public ShippingPalletRotationService(
        ILogger<ShippingPalletRotationService> logger,
        IReturnShippingPalletSelector returnShippingPalletSelector,
        IReturnShippingPalletService returnShippingPalletService
    ) {
        _logger = logger;
        _returnShippingPalletSelector = returnShippingPalletSelector;
        _returnShippingPalletService = returnShippingPalletService;
    }

    public void Return(ShippingStationCode stationCode) {
        _logger.LogInformation($"出荷パレット返却： 出荷作業場所[{stationCode}]");
        var returnableLocation = _returnShippingPalletSelector.SelectReturnShippingPallet(stationCode);
        if (returnableLocation == null) {
            return;
        }
        _returnShippingPalletService.Request(returnableLocation);
    }
}
