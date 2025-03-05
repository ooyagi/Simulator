using CommonItems.Models;
using Microsoft.Extensions.Logging;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnInventoryPalletSelector: IReturnInventoryPalletSelector
{
    private readonly ILogger<ReturnInventoryPalletSelector> _logger;
    private readonly ITempStorageLoader _tempStorageLoader;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly IShikakariStorageLoader _shikakariStorageLoader;

    public ReturnInventoryPalletSelector(
        ILogger<ReturnInventoryPalletSelector> logger,
        ITempStorageLoader tempStorageLoader,
        IShippingStorageLoader shippingStorageLoader,
        IShikakariStorageLoader shikakariStorageLoader
    ) {
        _logger = logger;
        _tempStorageLoader = tempStorageLoader;
        _shippingStorageLoader = shippingStorageLoader;
        _shikakariStorageLoader = shikakariStorageLoader;
    }

    /// <summary>
    /// 返却在庫パレット選択
    /// 
    /// 出荷作業場所において、返却可能な在庫パレットを選択する。
    /// 1. 空パレットがあれば選択する
    /// 2. 出荷パレットに積載出来ず仕掛パレットにも積載できないパレットがあれば選択する
    /// </summary>
    /// <param name="stationCode"></param>
    /// <returns></returns>
    public LocationCode? SelectReturnInventoryPallet(ShippingStationCode stationCode) {
        _logger.LogInformation($"返却在庫パレット選択： 出荷作業場所 [{stationCode}]");
        var emptyPallets = _tempStorageLoader.GetEmptyPallets(stationCode);
        if (0 < emptyPallets.Count()) {
            return emptyPallets.First().LocationCode;
        }
        var tempStorageItems = _tempStorageLoader.GetAvarableHinbans(stationCode).ToList();
        if (!tempStorageItems.Any()) {
            return null;
        }
        var shippingPalletsLoadebleInfos = _shippingStorageLoader.GetLoadableFrom(stationCode, tempStorageItems);
        var unuseableItems = tempStorageItems.Where(x => !shippingPalletsLoadebleInfos.Any(y => y.IsLoadableQuantityGreaterThan(x.Hinban, 1)));
        if (!unuseableItems.Any()) {
            return null;
        }
        var shikakariPAlletsLoadebleInfos = _shikakariStorageLoader.GetLoadableFrom(tempStorageItems);
        return  unuseableItems.Where(x => !shikakariPAlletsLoadebleInfos.Any(y => y.IsLoadableQuantityGreaterThan(x.Hinban, 1)))
            .OrderBy(x => x.LocationCode.Value)
            .FirstOrDefault()?
            .LocationCode;
    }
}
