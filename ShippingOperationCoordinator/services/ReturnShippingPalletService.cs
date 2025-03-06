using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnShippingPalletService: IChangeShippingPalletService
{
    private readonly ILogger<ReturnShippingPalletService> _logger;
    private readonly IReturnShippingPalletSelector _returnShippingPalletSelector;
    private readonly IReturnShippingPalletService _returnShippingPalletService;

    public ReturnShippingPalletService(
        ILogger<ReturnShippingPalletService> logger,
        IReturnShippingPalletSelector returnShippingPalletSelector,
        IReturnShippingPalletService returnShippingPalletService
    ) {
        _logger = logger;
        _returnShippingPalletSelector = returnShippingPalletSelector;
        _returnShippingPalletService = returnShippingPalletService;
    }

    public bool Change(ShippingStationCode stationCode) {
        return Return(stationCode);
    }
    public bool Return(ShippingStationCode stationCode) {
        _logger.LogInformation($"出荷パレット返却： 出荷作業場所[{stationCode}]");
        var returnableLocation = _returnShippingPalletSelector.SelectReturnShippingPallet(stationCode);
        if (returnableLocation == null) {
            return false;
        }
        _returnShippingPalletService.Request(returnableLocation);
        return true;
    }
}
