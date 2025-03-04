using CommonItems.Models;
using Microsoft.Extensions.Logging;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnShippingPalletSelector: IReturnShippingPalletSelector
{
    private readonly ILogger<ReturnShippingPalletSelector> _logger;
    private readonly IInventoryStorageLoader _inventoryStorageLoader;
    private readonly IShippingStrageLoader _shippingStrageLoader;

    public ReturnShippingPalletSelector(
        ILogger<ReturnShippingPalletSelector> logger,
        IShippingStrageLoader shippingStrageLoader,
        IInventoryStorageLoader inventoryStorageLoader
    ) {
        _logger = logger;
        _shippingStrageLoader = shippingStrageLoader;
        _inventoryStorageLoader = inventoryStorageLoader;
    }

    public LocationCode? SelectReturnShippingPallet(ShippingStationCode stationCode) {
        _logger.LogInformation($"返却出荷パレット選択： 出荷作業場所 [{stationCode}]");
        var shippingPallets = _shippingStrageLoader.All(stationCode);
        var returnablePallets = shippingPallets.FirstOrDefault(p => p.IsCompleted);
        if (returnablePallets != null) {
            return returnablePallets.LocationCode;
        }
        returnablePallets = shippingPallets.FirstOrDefault(p => {
            var tmp = _inventoryStorageLoader.IsPickable(stationCode, p.NextHinban);
            return !tmp;
        });
        if (returnablePallets != null) {
            return returnablePallets.LocationCode;
        }
        return null;
    }
}
