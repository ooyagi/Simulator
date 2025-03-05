using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnInventoryPalletService
{
    private readonly ILogger<ReturnInventoryPalletService> _logger;
    private readonly IReturnInventoryPalletSelector _returnInventoryPalletSelector;
    private readonly IReturnInventoryPalletService _returnInventoryPalletService;

    public ReturnInventoryPalletService(
        ILogger<ReturnInventoryPalletService> logger,
        IReturnInventoryPalletSelector returnInventoryPalletSelector,
        IReturnInventoryPalletService returnInventoryPalletService
    ) {
        _logger = logger;
        _returnInventoryPalletSelector = returnInventoryPalletSelector;
        _returnInventoryPalletService = returnInventoryPalletService;
    }

    public void Return(ShippingStationCode stationCode) {
        _logger.LogInformation($"在庫パレット返却： 出荷作業場所[{stationCode}]");
        var returnableLocation = _returnInventoryPalletSelector.SelectReturnInventoryPallet(stationCode);
        if (returnableLocation == null) {
            return;
        }
        _returnInventoryPalletService.Request(returnableLocation);
    }
}
