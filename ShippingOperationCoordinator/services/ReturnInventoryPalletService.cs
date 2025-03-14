using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnInventoryPalletService: IChangeInventoryPalletService
{
    private readonly ILogger<ReturnInventoryPalletService> _logger;
    private readonly ITempStorageLoader _tempStorageLoader;
    private readonly IReturnInventoryPalletSelector _returnInventoryPalletSelector;
    private readonly IReturnInventoryPalletService _returnInventoryPalletService;
    private readonly Services.ITakeInventoryPalletService _takeInventoryPalletService;

    public ReturnInventoryPalletService(
        ILogger<ReturnInventoryPalletService> logger,
        ITempStorageLoader tempStorageLoader,
        IReturnInventoryPalletSelector returnInventoryPalletSelector,
        IReturnInventoryPalletService returnInventoryPalletService,
        Services.ITakeInventoryPalletService takeInventoryPalletService
    ) {
        _logger = logger;
        _tempStorageLoader = tempStorageLoader;
        _returnInventoryPalletSelector = returnInventoryPalletSelector;
        _returnInventoryPalletService = returnInventoryPalletService;
        _takeInventoryPalletService = takeInventoryPalletService;
    }

    public bool Change(ShippingStationCode stationCode) {
        if (_tempStorageLoader.GetEmptyLocationCodes(stationCode).Any()) {
            _takeInventoryPalletService.Take(stationCode);
            return true;
        }
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
}
