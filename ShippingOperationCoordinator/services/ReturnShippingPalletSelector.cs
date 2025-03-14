using CommonItems.Models;
using Microsoft.Extensions.Logging;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnShippingPalletSelector: IReturnShippingPalletSelector
{
    private readonly ILogger<ReturnShippingPalletSelector> _logger;
    private readonly ITempStorageLoader _tempStorageLoader;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly IStorableHinbanLoader _storableHinbanLoader;
    private readonly ITakeShippingPalletSelector _takeShippingPalletSelector;

    public ReturnShippingPalletSelector(
        ILogger<ReturnShippingPalletSelector> logger,
        ITempStorageLoader tempStorageLoader,
        IShippingStorageLoader shippingStorageLoader,
        IStorableHinbanLoader storableHinbanLoader,
        ITakeShippingPalletSelector takeShippingPalletSelector
    ) {
        _logger = logger;
        _tempStorageLoader = tempStorageLoader;
        _shippingStorageLoader = shippingStorageLoader;
        _storableHinbanLoader = storableHinbanLoader;
        _takeShippingPalletSelector = takeShippingPalletSelector;
    }

    public LocationCode? SelectReturnShippingPallet(ShippingStationCode stationCode) {
        _logger.LogInformation($"返却出荷パレット選択： 出荷作業場所 [{stationCode}]");

        // 完了パレットは返却する
        var shippingPallets = _shippingStorageLoader.All(stationCode);
        var completedPallet = shippingPallets.FirstOrDefault(p => p.IsCompleted);
        if (completedPallet != null) {
            return completedPallet.LocationCode;
        }
        // 次の利用品番が手動積替えであれば返却する
        var handTransferPallet = shippingPallets.FirstOrDefault(x => x.NextHinban == null || !_storableHinbanLoader.IsStorable(x.NextHinban));
        if (handTransferPallet != null) {
            return handTransferPallet.LocationCode;
        }
        // 現在の一時置き場の状況で積み込める仕掛パレットがない場合は返却しない
        if (!_takeShippingPalletSelector.CheckEnableShippingPalletInShikakariStorage(stationCode)) {
            _logger.LogDebug("入れ替えに有効な仕掛パレット置き場に無いため在庫パレットの入れ替わりをまちます");
            return null;
        }
        _logger.LogTrace("未完了の出荷パレットが見つかりませんでした");
        var returnablePallets = shippingPallets
            .FirstOrDefault(p => {
                if (p.NextHinban == null) {
                    return true;
                }
                var tmp = _tempStorageLoader.IsPickable(stationCode, p.NextHinban);
                return !tmp;
            });
        return returnablePallets?.LocationCode;
    }
}
