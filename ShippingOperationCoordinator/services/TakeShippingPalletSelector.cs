using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;
using ShippingOperationCoordinator.Models;

namespace ShippingOperationCoordinator.Services;

public class TakeShippingPalletSelector: ITakeShippingPalletSelector
{
    private readonly ILogger<TakeShippingPalletSelector> _logger;
    private readonly IShikakariStorageLoader _shikakariStorageLoader;
    private readonly ITempStorageLoader _tempStorageLoader;
    private readonly IInventoryStorageLoader _inventoryStorageLoader;
    private readonly IWorkOrderLoader _workOrderLoader;

    public TakeShippingPalletSelector(
        ILogger<TakeShippingPalletSelector> logger,
        IShikakariStorageLoader shikakariStorageLoader,
        ITempStorageLoader tempStorageLoader,
        IInventoryStorageLoader inventoryStorageLoader,
        IWorkOrderLoader workOrderLoader
    ) {
        _logger = logger;
        _shikakariStorageLoader = shikakariStorageLoader;
        _tempStorageLoader = tempStorageLoader;
        _inventoryStorageLoader = inventoryStorageLoader;
        _workOrderLoader = workOrderLoader;
    }

    public ShippingPalletID? SelectTakeShippingPallet(ShippingStationCode stationCode) {
        _logger.LogInformation($"出荷パレット取り寄せ候補選定開始： 出荷作業場所 [{stationCode}]");
        try {
            // 一時置き場の在庫を取得
            var tempStorageItems = _tempStorageLoader.GetAvarableHinbans(stationCode).Select(x => new TemporaryStoragePalletInfo(x.LocationCode, x.Hinban, x.Quantity)).ToList();
            // 一時置き場の在庫だけで完了する出荷パレット
            var completablePalletId = FilterShippingPalletsCompletedByTempInventory(tempStorageItems);
            if (completablePalletId != null) {
                return completablePalletId;
            }
            // 仕掛パレットの積込情報を取得
            var shikakariPallets = _shikakariStorageLoader.GetLoadableFrom(tempStorageItems);
            if (!shikakariPallets.Any()) {
                return null;
            }

            // 一時置き場の在庫だけを利用して一時置き場の在庫パレットを使い切れる（空パレットに出来る）品番を使用する
            var canBeEmptiedPalletId = FilterInventoryPalletsThatCanBeEmptied(tempStorageItems, shikakariPallets);
            if (canBeEmptiedPalletId != null) {
                return canBeEmptiedPalletId;
            }
            // 一時置き場の在庫パレットの内、積み込み可能な仕掛パレット数が少ない品番が次回積込予定となっている出荷パレット
            var palletIdWhereNextHinbanIsDeterminedByFewLoadableShikakariPallet = FilterShippingPalletWithFewLoadableShikakariPallets(tempStorageItems, shikakariPallets);
            if (palletIdWhereNextHinbanIsDeterminedByFewLoadableShikakariPallet != null) {
                return palletIdWhereNextHinbanIsDeterminedByFewLoadableShikakariPallet;
            }
            // 一時置き場の在庫パレットの内、在庫数が少ない品番が次回積込予定となっている出荷パレット
            var palletIdWhereNextHinbanIsDeterminedByLowInventoryQuantity = FilterShippingPalletWithLowInventoryQuantity(tempStorageItems, shikakariPallets);
            if (palletIdWhereNextHinbanIsDeterminedByLowInventoryQuantity != null) {
                return palletIdWhereNextHinbanIsDeterminedByLowInventoryQuantity;
            }
            // 現在一時置き場に置かれている在庫の次に他の出荷作業場所の一時置き場に存在する品番を利用する
            var palletIdWhereNextHinbanIsUsedInOtherShippingStationTemporaryStorage = FilterShippingPalletWithNextHinbanUsedInOtherStationTempStorage(stationCode, shikakariPallets);
            if (palletIdWhereNextHinbanIsUsedInOtherShippingStationTemporaryStorage != null) {
                return palletIdWhereNextHinbanIsUsedInOtherShippingStationTemporaryStorage;
            }
            // 仕掛パレットのブロック要因となる品番の内、在庫のある品番を使用する仕掛パレットを抽出する
            var shikakariPalletLoadableInfoFilterdByEnableBlockHinban = FilterBlockHinbanStockExists(shikakariPallets);
            if (shikakariPalletLoadableInfoFilterdByEnableBlockHinban.Any()) {
                shikakariPallets = shikakariPalletLoadableInfoFilterdByEnableBlockHinban;
                // ブロック要因となる品番の在庫を使い切れる
                var palletIdWhereBlockHinbanStockCanBeExhausted = FilterShippingPalletWhereBlockHinbanStockCanBeExhausted(shikakariPallets);
                // ブロック要因となる品番で（残りの生産計画全体で）該当パレット以外での利用予定が無い品番を使用する
                var palletIdWhereBlockHinbanIsNotUsedElsewhere = FilterShippingPalletWhereBlockHinbanIsNotUsedElsewhere(shikakariPallets);
            }
            // 今後積込予定の品番の種類が少なく今後積込予定の数が少ない
            return FilterShippingPalletWithFewFutureLoadableHinbanTypes(shikakariPallets);
        } catch (Exception ex) {
            _logger.LogError(ex, "出荷パレット取り寄せ候補選定中にエラーが発生しました");
        }
        _logger.LogWarning("取り寄せ可能な出荷パレット候補が見つかりませんでした");
        return ShippingPalletID.CustomPaletteID;
    }

    /// <summary>
    /// 一時置き場の在庫で完了できる出荷パレットの抽出
    /// 
    /// 積み込み可能な品番x数量のリストを用いて仕掛パレット起き場に対して完了可能な出荷パレットを問い合わせる。
    /// 完了可能な出荷パレットが複数ある場合は、積替えステップが最も小さいものを選択する。
    /// </summary>
    /// <remarks>積替え先の品番が見つからない場合はnullを返す</remarks>
    private ShippingPalletID? FilterShippingPalletsCompletedByTempInventory(IEnumerable<IInventoryPalletInfo> tempStorageItems) {
        var completablePallets = _shikakariStorageLoader.FilterCompletableBy(tempStorageItems);
        if (!completablePallets.Any()) {
            return null;
        }
        var targetPallet = completablePallets.OrderBy(x => x.Step).First();
        return targetPallet.ShippingPalletID;
    }
    /// <summary>
    /// 使い切れる在庫パレットの抽出
    /// 
    /// 以下の手順で積替え品番、積替え元、積み替え先を求める。
    /// 1. 仕掛パレット置き場から出荷パレットの積み込み情報のリストを取得する
    /// 2. 取得した積み込み情報のリストに問い合わせ、一時置き場の在庫を使い切れる出荷パレットを求める
    /// 
    /// 使い切れる在庫パレットが複数ある場合は使い切るまでのSTEP数が少ないものを選択する
    /// </summary>
    /// <remarks>積替え元の在庫パレット／積替え先の出荷パレットが見つからない場合はnullを返す</remarks>
    private ShippingPalletID? FilterInventoryPalletsThatCanBeEmptied(IEnumerable<IInventoryPalletInfo> tempStorageItems, IEnumerable<IShikakariPalletLoadableHinbanInfo> shikakariPallets) {
        var targetPallets = shikakariPallets.SelectMany(x => x.GetEmptiablePallets(tempStorageItems).Select(y => (x.ShippingPalletID, y.Hinban, y.EmptiableStep)));
        if (!targetPallets.Any()) {
            return null;
        }
        return targetPallets.OrderBy(x => x.EmptiableStep).First().ShippingPalletID;
    }
    /// <summary>
    /// 一時置き場の在庫パレットの内、積み込み可能な仕掛パレット数が少ない品番が次回積込予定となっている出荷パレットを抽出する
    /// 
    /// 複数のパレットが該当する場合は利用する品番が少ない物を選択する
    /// 複数のパレットが該当する場合は残り積替えステップが最も小さい物を選択する
    /// </summary>
    private ShippingPalletID? FilterShippingPalletWithFewLoadableShikakariPallets(IEnumerable<IInventoryPalletInfo> tempStorageItems, IEnumerable<IShikakariPalletLoadableHinbanInfo> shikakariPallets) {
        var targetTempPallet = tempStorageItems.Select(x => new { x.Hinban, x.Quantity, Count = shikakariPallets.Count(y => y.IsLoadableQuantityGreaterThan(x.Hinban, 1))})
            .Where(x => 0 < x.Count)
            .OrderBy(x => x.Count)
            .ThenBy(x => x.Quantity)
            .FirstOrDefault();
        var targetPallets = shikakariPallets.Where(x => x.NextHinban == targetTempPallet?.Hinban);
        if (!targetPallets.Any()) {
            return null;
        }
        return targetPallets.OrderBy(x => x.FutureLoadableHinbanTypeCount).ThenBy(x => x.RemainStep).First().ShippingPalletID;
    }
    /// <summary>
    /// 一時置き場の在庫パレットの内、在庫数が少ない品番が次回積込予定となっている出荷パレットを抽出する
    /// 
    /// 複数のパレットが該当する場合は利用する品番が少ない物を選択する
    /// 複数のパレットが該当する場合は残り積替えステップが最も小さい物を選択する
    /// </summary>
    private ShippingPalletID? FilterShippingPalletWithLowInventoryQuantity(IEnumerable<IInventoryPalletInfo> tempStorageItems, IEnumerable<IShikakariPalletLoadableHinbanInfo> shikakariPallets) {
        var targetTempPallet = tempStorageItems.OrderBy(x => x.Quantity).FirstOrDefault();
        if (targetTempPallet == null) {
            return null;
        }
        var targetPallets = shikakariPallets.Where(x => x.NextHinban == targetTempPallet.Hinban);
        if (!targetPallets.Any()) {
            return null;
        }
        return targetPallets.OrderBy(x => x.FutureLoadableHinbanTypeCount).ThenBy(x => x.RemainStep).First().ShippingPalletID;
    }
    /// <summary>
    /// 現在一時置き場に置かれている在庫の次に他の出荷作業場所の一時置き場に存在する品番を利用するパレットを抽出する
    /// 
    /// 複数のパレットが該当する場合は利用する品番が少ない物を選択する
    /// 複数のパレットが該当する場合は残り積替えステップが最も小さい物を選択する
    /// </summary>
    private ShippingPalletID? FilterShippingPalletWithNextHinbanUsedInOtherStationTempStorage(ShippingStationCode stationCode, IEnumerable<IShikakariPalletLoadableHinbanInfo> shikakariPallets) {
        var tempStorageItemsInOtherStation = _tempStorageLoader.GetAvarableHinbansInOtherStation(stationCode);
        var targetPallets = shikakariPallets.Where(x => tempStorageItemsInOtherStation.Any(y => x.BlockHinban == y.Hinban));
        if (!targetPallets.Any()) {
            return null;
        }
        return targetPallets.OrderBy(x => x.FutureLoadableHinbanTypeCount).ThenBy(x => x.RemainStep).First().ShippingPalletID;
    }
    /// <summary>
    /// 仕掛パレットのブロック要因となる品番の内、在庫のある品番を絞り込む
    /// </summary>
    private IEnumerable<IShikakariPalletLoadableHinbanInfo> FilterBlockHinbanStockExists(IEnumerable<IShikakariPalletLoadableHinbanInfo> shikakariPallets) {
        return shikakariPallets.Where(x => _inventoryStorageLoader.IsExists(x.BlockHinban)).ToList();
    }
    /// <summary>
    /// ブロック要因となる品番の在庫を使い切れる出荷パレットを抽出する
    /// </summary>
    ShippingPalletID? FilterShippingPalletWhereBlockHinbanStockCanBeExhausted(IEnumerable<IShikakariPalletLoadableHinbanInfo> filterdPallets) {
        var targetPallets = filterdPallets.Where(x => _inventoryStorageLoader.IsUseup(x.BlockHinban, x.BlockHinbanLoadableCount ));
        if (!targetPallets.Any()) {
            return null;
        }
        return targetPallets.OrderBy(x => x.FutureLoadableHinbanTypeCount).ThenBy(x => x.RemainStep).First().ShippingPalletID;
    }
    /// <summary>
    /// ブロック要因となる品番で（残りの生産計画全体で）該当パレット以外での利用予定が無い品番を使用する
    /// </summary>
    ShippingPalletID? FilterShippingPalletWhereBlockHinbanIsNotUsedElsewhere(IEnumerable<IShikakariPalletLoadableHinbanInfo> filterdPallets) {
        var targetPallets = filterdPallets.Where(x => _workOrderLoader.GetUsageWorkOrderIdByHinban(x.BlockHinban).Count() == 1);
        if (!targetPallets.Any()) {
            return null;
        }
        return targetPallets.OrderBy(x => x.FutureLoadableHinbanTypeCount).ThenBy(x => x.RemainStep).First().ShippingPalletID;
    }
    /// <summary>
    /// 今後積込予定の品番の種類が少なく今後積込予定の数が少ないパレットの抽出
    /// </summary>
    ShippingPalletID? FilterShippingPalletWithFewFutureLoadableHinbanTypes(IEnumerable<IShikakariPalletLoadableHinbanInfo> shikakariPallets) {
        return shikakariPallets.OrderBy(x => x.FutureLoadableHinbanTypeCount).ThenBy(x => x.RemainStep).First().ShippingPalletID;
    }
}
