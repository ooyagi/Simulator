using CommonItems.Models;
using Microsoft.Extensions.Logging;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnInventoryPalletSelector: IReturnInventoryPalletSelector
{
    private readonly ILogger<ReturnInventoryPalletSelector> _logger;
    private readonly ITempStorageLoader _tempStorageLoader;
    private readonly IShippingPalletLoader _shippingPalletLoader;

    public ReturnInventoryPalletSelector(
        ILogger<ReturnInventoryPalletSelector> logger,
        ITempStorageLoader tempStorageLoader,
        IShippingPalletLoader shippingPalletLoader
    ) {
        _logger = logger;
        _tempStorageLoader = tempStorageLoader;
        _shippingPalletLoader = shippingPalletLoader;
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
        // 全体の積載可能情報を取得
        // 他の出荷作業場所にあるパレットは、他の出荷作業場所にある在庫＋現在の出荷作業場所の在庫で判断する必要があるため
        // 他の出荷作業場所の在庫を渡す必要がある
        // 現在は行っていないため、精度を落として検証している
        var shippingPalletsLoadebleInfos = _shippingPalletLoader.GetLoadableFrom(tempStorageItems);
        var unuseableItems = tempStorageItems.Where(x => !shippingPalletsLoadebleInfos.Any(y => y.IsLoadableQuantityGreaterThan(x.Hinban, 1)));
        if (!unuseableItems.Any()) {
            return null;
        }
        return  unuseableItems
            .OrderBy(x => x.LocationCode.Value)
            .FirstOrDefault()?
            .LocationCode;
    }
    /// <summary>
    /// 空在庫パレット選択
    /// </summary>
    public LocationCode? SelectEmptyInventoryPallet(ShippingStationCode stationCode) {
        _logger.LogInformation($"空在庫パレット選択： 出荷作業場所 [{stationCode}]");
        var emptyPallets = _tempStorageLoader.GetEmptyPallets(stationCode);
        if (0 < emptyPallets.Count()) {
            return emptyPallets.First().LocationCode;
        }
        return null;
    }
}
