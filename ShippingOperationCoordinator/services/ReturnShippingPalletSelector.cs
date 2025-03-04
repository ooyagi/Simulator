using CommonItems.Models;
using Microsoft.Extensions.Logging;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnShippingPalletSelector: IReturnShippingPalletSelector
{
    private readonly ILogger<ReturnShippingPalletSelector> _logger;
    private readonly ITempStorageLoader _tempStorageLoader;
    private readonly IShippingStorageLoader _shippingStorageLoader;

    public ReturnShippingPalletSelector(
        ILogger<ReturnShippingPalletSelector> logger,
        ITempStorageLoader tempStorageLoader,
        IShippingStorageLoader shippingStorageLoader
    ) {
        _logger = logger;
        _tempStorageLoader = tempStorageLoader;
        _shippingStorageLoader = shippingStorageLoader;
    }

    public LocationCode? SelectReturnShippingPallet(ShippingStationCode stationCode) {
        _logger.LogInformation($"返却出荷パレット選択： 出荷作業場所 [{stationCode}]");

        var shippingPallets = _shippingStorageLoader.All(stationCode);
        var completedPallet = shippingPallets.FirstOrDefault(p => p.IsCompleted);
        if (completedPallet != null) {
            return completedPallet.LocationCode;
        }
        var returnablePallets = shippingPallets.FirstOrDefault(p => {
            var tmp = _tempStorageLoader.IsPickable(stationCode, p.NextHinban);
            return !tmp;
        });
        if (returnablePallets != null) {
            return returnablePallets.LocationCode;
        }
        return null;
    }
}
