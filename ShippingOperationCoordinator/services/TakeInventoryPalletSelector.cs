using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

public class TakeInventoryPalletSelector: ITakeInventoryPalletSelector
{
    private readonly ILogger<TakeInventoryPalletSelector> _logger;
    private readonly IStorableHinbanLoader _storableHinbanLoader;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly IShikakariStorageLoader _shikakariStorageLoader;
    private readonly ITempStorageLoader _tempStorageLoader;
    private readonly IInventoryStorageLoader _inventoryStorageLoader;

    public TakeInventoryPalletSelector(
        ILogger<TakeInventoryPalletSelector> logger,
        IStorableHinbanLoader storableHinbanLoader,
        IShippingStorageLoader shippingStorageLoader,
        IShikakariStorageLoader shikakariStorageLoader,
        ITempStorageLoader tempStorageLoader,
        IInventoryStorageLoader inventoryStorageLoader
    ) {
        _logger = logger;
        _storableHinbanLoader = storableHinbanLoader;
        _shippingStorageLoader = shippingStorageLoader;
        _shikakariStorageLoader = shikakariStorageLoader;
        _tempStorageLoader = tempStorageLoader;
        _inventoryStorageLoader = inventoryStorageLoader;
    }

    /// <summary>
    /// 在庫パレット取り寄せ候補選定
    /// 
    /// 出荷パレット、仕掛パレットの積載可能情報を元に在庫パレットを取り寄せる品番を選定する
    /// </summary>
    /// <remarks>
    /// 在庫パレットを完了に出来るかどうかについても判定したかったが
    /// 在庫パレットの搬送完了前に他品番の積替えや出荷パレットの入れ替え等によって状況が変わるため
    /// 到着時点の状態を予想することが困難なため判定を行わない。
    /// 将来的に搬送開始時点で何らかのリソースをロックするなどして状況を固定化することで判定を行うことが考えられるが
    /// その間、現場作業が待ちになってしまうリスクがあるため、現時点では実施の可能性は低いと考えている。
    /// </remarks>
    public Hinban? SelectTakeInventoryPallet(ShippingStationCode stationCode) {
        _logger.LogInformation($"在庫パレット取り寄せ候補選定開始： 出荷作業場所 [{stationCode}]");
        try {
            IEnumerable<IInventoryPalletInfo> candidates;
            // 一時置き場、在庫パレット置き場の在庫情報を取得
            // 現時点の出荷パレット、仕掛パレットの積載可能情報を取得
            var inventoryItems = _inventoryStorageLoader.GetStoragedItems();
            var tempStorageItems = _tempStorageLoader.GetAvarableHinbans(stationCode);
            var shippingPalletLoadableInfo = _shippingStorageLoader.GetLoadableFrom(stationCode, tempStorageItems);
            var shikakariPalletLoadableInfo = _shikakariStorageLoader.GetLoadableFrom(tempStorageItems);

            // 積込可能な在庫パレットをフィルタリング
            // あえて在庫がないパレットも返す（メソッド側の注釈を確認）
            candidates = FilterLoadableItems(stationCode, inventoryItems, tempStorageItems, shippingPalletLoadableInfo, shikakariPalletLoadableInfo);

            // 出荷パレットを完了に出来る在庫パレットをフィルタリング
            var itemsCanFinishWorkOnShippingPallet = FilterInventoryPalletsByShippingPalletCompletion(candidates, shippingPalletLoadableInfo);
            if (itemsCanFinishWorkOnShippingPallet.Count() == 1) {
                return itemsCanFinishWorkOnShippingPallet.First().Hinban;
            } else if (0 < itemsCanFinishWorkOnShippingPallet.Count()) {
                candidates = itemsCanFinishWorkOnShippingPallet;
            }
            // 仕掛パレットを完了に出来る在庫パレットをフィルタリング
            var itemsCanFinishWorkOnShikakariPallet = FilterInventoryPalletsByMoreShikakariPalletCompletion(candidates, shikakariPalletLoadableInfo);
            if (itemsCanFinishWorkOnShikakariPallet.Count() == 1) {
                return itemsCanFinishWorkOnShikakariPallet.First().Hinban;
            } else if (0 < itemsCanFinishWorkOnShikakariPallet.Count()) {
                candidates = itemsCanFinishWorkOnShikakariPallet;
            }
            // 多くの出荷パレット／仕掛パレットをブロックしている品番の在庫パレットを選定
            var itemsMostPalletsBlockingFactor = FilterInventoryPalletsByMoreBlockingFactor(candidates, shippingPalletLoadableInfo, shikakariPalletLoadableInfo);
            if (itemsMostPalletsBlockingFactor.Count() == 1) {
                return itemsMostPalletsBlockingFactor.First().Hinban;
            } else if (0 < itemsMostPalletsBlockingFactor.Count()) {
                candidates = itemsMostPalletsBlockingFactor;
            }
            if (!candidates.Any()) {
                _logger.LogWarning("取り寄せ可能な在庫パレット候補が見つかりませんでした");
                return null;
            }
            return candidates.OrderBy(x => x.Quantity).First().Hinban;
        } catch (Exception ex) {
            _logger.LogError(ex, "出荷パレット取り寄せ候補選定中にエラーが発生しました");
        }
        _logger.LogWarning("取り寄せ可能な出荷パレット候補が見つかりませんでした");
        return null;
    }

    /// <summary>
    /// 積込可能な在庫パレットをフィルタリング
    /// 
    /// 一時置き場に置かれている品番は除外する
    /// 在庫パレットの内、出荷パレット or 仕掛パレットで次回積込品番またはブロック品番に指定されている品番を持つ在庫パレットを抽出
    /// 同一品番を積載したパレットが複数ある場合は在庫数が少ないパレットを優先して抽出
    /// 
    /// 本番では上記の動作だが、搬送回数の試算時点では適切な搬入が行われているものとそて試算する
    /// このため、在庫パレットに必要なパレットが存在しない場合はフル積載のパレットが存在するものとして選定を行う
    /// ※取り寄せ処理側でも不足しているパレットを自動作成する処理を入れる
    /// 
    /// 最後に在庫パレット置き場に配置する品番のみに絞り込む
    /// </summary>
    private IEnumerable<IInventoryPalletInfo> FilterLoadableItems(ShippingStationCode stationCode, IEnumerable<IInventoryPalletInfo> inventoryItems, IEnumerable<IInventoryPalletInfo> tempItems, IEnumerable<IShippingPalletLoadableHinbanInfo> shippingPalletLoadableInfo, IEnumerable<IShikakariPalletLoadableHinbanInfo> shikakariPalletLoadableInfo) {
        // - 不足品番の自動補充 -
        var nextItems = shippingPalletLoadableInfo.Select(x => x.NextHinban).Concat(shikakariPalletLoadableInfo.Select(x => x.NextHinban)).Distinct().ToList();
        var blockItems = shippingPalletLoadableInfo.Select(x => x.BlockHinban).Concat(shikakariPalletLoadableInfo.Select(x => x.BlockHinban)).Distinct().ToList();
        var neededItems = nextItems.Concat(blockItems).Distinct().Where(x => x != null).ToList();
        var missingItems = neededItems.Where(x => x != null && !inventoryItems.Any(y => y.Hinban == x));
        var missingPallets = missingItems.Select(x => new InventoryPalletInfo(LocationCode.Default, x!, 14)).ToList();
        inventoryItems = inventoryItems.Concat(missingPallets);
        // ----- ここまで -----

        var loadableItems = inventoryItems
            .Where(x => !tempItems.Any(y => y.Hinban == x.Hinban))
            .Where(x => shippingPalletLoadableInfo.Any(y => y.NextHinban == x.Hinban)
                || shikakariPalletLoadableInfo.Any(y => y.NextHinban == x.Hinban)
                || shippingPalletLoadableInfo.Any(y => y.BlockHinban == x.Hinban)
                || shikakariPalletLoadableInfo.Any(y => y.BlockHinban == x.Hinban)
            )
            .Where(x => _storableHinbanLoader.IsStorable(x.Hinban)
                && _tempStorageLoader.IsTakable(stationCode, x.Hinban))
            .ToList();
        return loadableItems
            .GroupBy(x => x.Hinban)
            .Select(x => x.OrderBy(y => y.Quantity).First());
    }
    /// <summary>
    /// 出荷パレットのを完了に出来る在庫パレットをフィルタリング
    /// </summary>
    private IEnumerable<IInventoryPalletInfo> FilterInventoryPalletsByShippingPalletCompletion(IEnumerable<IInventoryPalletInfo> inventoryItems, IEnumerable<IShippingPalletLoadableHinbanInfo> shippingPalletLoadableInfo) {
        // 完了に出来る出荷パレット数をカウント
        var inventoryItemsWithCompletableCount = inventoryItems.Select(x => {
                var Count = shippingPalletLoadableInfo.Count(y => y.IsCompletableBy(x));
                return new { item = x, Count};
            })
            .Where(x => 0 < x.Count);
        if (!inventoryItemsWithCompletableCount.Any()) {
            return Enumerable.Empty<IInventoryPalletInfo>();
        }
        // 完了に出来る出荷パレット数が最大の在庫パレットを選定
        var groupedItems = inventoryItemsWithCompletableCount.GroupBy(x => x.Count);
        return groupedItems.OrderByDescending(x => x.Key).First().Select(x => x.item).ToList();
    }
    /// <summary>
    /// 仕掛パレットを完了に出来る在庫パレットをフィルタリング
    /// </summary>
    private IEnumerable<IInventoryPalletInfo> FilterInventoryPalletsByMoreShikakariPalletCompletion(IEnumerable<IInventoryPalletInfo> inventoryItems, IEnumerable<IShikakariPalletLoadableHinbanInfo> shikakariPalletLoadableInfo) {
        // 完了に出来る仕掛パレット数をカウント
        var inventoryItemsWithCompletableCount = inventoryItems.Select(x => {
                var Count = shikakariPalletLoadableInfo.Count(y => y.IsCompletableBy(x));
                return new { item = x, Count};
            })
            .Where(x => 0 < x.Count);
        if (!inventoryItemsWithCompletableCount.Any()) {
            return Enumerable.Empty<IInventoryPalletInfo>();
        }
        // 完了に出来る仕掛パレット数が最大の在庫パレットを選定
        var groupedItems = inventoryItemsWithCompletableCount.GroupBy(x => x.Count);
        return groupedItems.OrderByDescending(x => x.Key).First().Select(x => x.item).ToList();
    }
    /// <summary>
    /// 出荷パレット、仕掛パレットの BlockHinban に指定されている数が最も多い在庫パレットを選定
    /// 
    /// 在庫数不足でブロックが解除できない可能性もあるが、何れにせよそのパレットを空にしないと作業が進められないため
    /// 在庫数や利用数を考慮せずに最も多くの出荷パレットの BlockHinban に指定されている
    /// </summary>
    private IEnumerable<IInventoryPalletInfo> FilterInventoryPalletsByMoreBlockingFactor(IEnumerable<IInventoryPalletInfo> inventoryItems, IEnumerable<IShippingPalletLoadableHinbanInfo> shippingPalletLoadableInfo, IEnumerable<IShikakariPalletLoadableHinbanInfo> shikakariPalletLoadableInfo) {
        var shippingPalletUnblockableItems = inventoryItems.Select(x => {
                var Count = shippingPalletLoadableInfo.Count(y => y.BlockHinban == x.Hinban);
                return new { item = x, Count};
            })
            .Where(x => 0 < x.Count).ToList();
        var shikakariPalletUnblockableItems = inventoryItems.Select(x => {
                var Count = shikakariPalletLoadableInfo.Count(y => y.BlockHinban == x.Hinban);
                return new { item = x, Count};
            })
            .Where(x => 0 < x.Count).ToList();
        // 出荷パレット、仕掛パレットの積込可能数が最大の在庫パレットを選定
        var unblockableItems = shippingPalletUnblockableItems.Select(x => {
            var shikakariPallet = shikakariPalletUnblockableItems.FirstOrDefault(y => y.item.Hinban == x.item.Hinban);
            var totalCount = x.Count + (shikakariPallet?.Count ?? 0);
            return new { x.item, Count = totalCount};
        })
        .Where(x => 0 < x.Count )
        .ToList();
        if (!unblockableItems.Any()) {
            return Enumerable.Empty<IInventoryPalletInfo>();
        }
        var groupedItems = unblockableItems.GroupBy(x => x.Count);
        return groupedItems.OrderByDescending(x => x.Key).First().Select(x => x.item).ToList();
    }
    // 試算のための仮想在庫パレット
    record InventoryPalletInfo(LocationCode LocationCode, Hinban Hinban, int Quantity = 14): IInventoryPalletInfo;
}
