using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnInventoryPalletService: IChangeInventoryPalletService
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

    public bool Change(ShippingStationCode stationCode) {
        return Return(stationCode);
    }

    public bool ChangeEmptyPallet(ShippingStationCode stationCode) {
        return ReturnEmptyPallet(stationCode);
    }
    public bool Return(ShippingStationCode stationCode) {
        _logger.LogInformation($"在庫パレット返却： 出荷作業場所[{stationCode}]");
        var returnableLocation = _returnInventoryPalletSelector.SelectReturnInventoryPallet(stationCode);
        if (returnableLocation == null) {
            return false;
        }
        _returnInventoryPalletService.Request(returnableLocation);
        return true;
    }
    public bool ReturnEmptyPallet(ShippingStationCode stationCode) {
        _logger.LogInformation($"空在庫パレット返却： 出荷作業場所[{stationCode}]");
        var returnableLocation = _returnInventoryPalletSelector.SelectEmptyInventoryPallet(stationCode);
        if (returnableLocation == null) {
            return false;
        }
        _returnInventoryPalletService.Request(returnableLocation);
        return true;
    }

    void IChangeInventoryPalletService.Change(ShippingStationCode stationCode)
    {
        throw new NotImplementedException();
    }
}
