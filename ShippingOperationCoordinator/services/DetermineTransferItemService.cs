using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;
using ShippingOperationCoordinator.Models;

namespace ShippingOperationCoordinator.Services;

/// <summary>
/// 一時置き場に置かれた在庫パレットから出荷パレット置き場に置かれた出荷パレットに積替える品番を決定する
/// </summary>
class DetermineTransferItemService: IDetermineTransferItemService
{
    ILogger<DetermineTransferItemService> _logger;
    private readonly ITempStorageLoader _tempStorageLoader;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly IShikakariStorageLoader _ShikakariStorageLoader;

    public DetermineTransferItemService(
        ILogger<DetermineTransferItemService> logger,
        ITempStorageLoader tempStorageLoader,
        IShippingStorageLoader shippingStorageLoader,
        IShikakariStorageLoader ShikakariStorageLoader)
    {
        _logger = logger;
        _tempStorageLoader = tempStorageLoader;
        _shippingStorageLoader = shippingStorageLoader;
        _ShikakariStorageLoader = ShikakariStorageLoader;
    }

    /// <summary>
    /// 与えられた在庫パレットのリストから、積替え指示に基づいて転送対象の品番 (Hinban) を決定する
    /// </summary>
    public TransferDirection DetermineTransferHinban(ShippingStationCode stationCode) {
        _logger.LogInformation("積替え対象の品番を決定します");
        try {
            var availableHinbans = _tempStorageLoader.GetAvarableHinbans(stationCode).Select(x => new TemporaryStoragePalletInfo(x.LocationCode, x.Hinban, x.Quantity)).ToList();
            var shippingPalletCompletedCandidate = FilterShippingPalletsCompletedByTempInventory(stationCode, availableHinbans);
            if (shippingPalletCompletedCandidate != null) {
                return shippingPalletCompletedCandidate;
            }
            var inventoryPalletEmptiableCandidates = FilterInventoryPalletsThatCanBeEmptied(stationCode, availableHinbans);
            if (inventoryPalletEmptiableCandidates != null) {
                return inventoryPalletEmptiableCandidates;
            }
            var inventoryPalletNotUsedCandidates = FilterInventoryPalletsNotUsedInShikakariStorage(stationCode, availableHinbans);
            if (inventoryPalletNotUsedCandidates != null) {
                return inventoryPalletNotUsedCandidates;
            }
            var shippingPalletFewTransferCandidates = FilterShippingPalletsWithFewTransfers(stationCode, availableHinbans);
            if (shippingPalletFewTransferCandidates != null) {
                return shippingPalletFewTransferCandidates;
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "積替え対象の品番を決定する際にエラーが発生しました");
        }
        _logger.LogWarning("積替え可能な品番が見つかりませんでした");
        return new TransferDirection(Hinban.Default, LocationCode.Default, LocationCode.Default);
    }

    /// <summary>
    /// 一時置き場の在庫で完了できる出荷パレットを抽出するフィルタ
    /// 
    /// 積み込み可能な品番x数量のリストを用いて一時置き場に対して完了可能な出荷パレットを問い合わせる。
    /// 完了可能な出荷パレットが複数ある場合は、積替えステップが最も小さいものを選択する。
    /// </summary>
    /// <remarks>積替え先の品番が見つからない場合はnullを返す</remarks>
    private TransferDirection? FilterShippingPalletsCompletedByTempInventory(ShippingStationCode stationCode, IEnumerable<TemporaryStoragePalletInfo> pallets) {
        var completablePallets = _shippingStorageLoader.FilterCompletableBy(stationCode, pallets);
        if (!completablePallets.Any()) {
            return null;
        }
        var targetPallet = completablePallets.OrderBy(x => x.RemainStep).First();
        var pickupTarget = pallets.First(x => x.Hinban == targetPallet.NextHinban);
        return new TransferDirection(targetPallet.NextHinban, pickupTarget.LocationCode, targetPallet.LocationCode);
    }
    /// <summary>
    /// 使い切れる在庫パレットの積替え判断
    /// 
    /// 以下の手順で積替え品番、積替え元、積み替え先を求める。
    /// 1. 出荷パレット置き場から出荷パレットの積み込み情報のリストを取得する
    /// 2. 取得した積み込み情報のリストに問い合わせ、使い切れる在庫パレットのリストを取得する
    /// 
    /// 積み替え元の選択
    /// - 使い切れる在庫パレットが複数ある場合は在庫数が少ないものを返す
    /// 積替え先の選択
    /// - 積替え先の出荷パレットが複数ある場合は積替えステップが最も小さいものを選択する
    /// </summary>
    /// <remarks>積替え元の在庫パレット／積替え先の出荷パレットが見つからない場合はnullを返す</remarks>
    private TransferDirection? FilterInventoryPalletsThatCanBeEmptied(ShippingStationCode stationCode, IEnumerable<TemporaryStoragePalletInfo> pallets) {
        var shippingPallets = _shippingStorageLoader.GetLoadableFrom(stationCode, pallets);
        if (!shippingPallets.Any()) {
            return null;
        }
        var emptiablePallets = pallets.Where(tmpPallet => {
            return shippingPallets.Any(shpPallet => shpPallet.IsLoadableQuantityGreaterThan(tmpPallet.Hinban, tmpPallet.Quantity));
        });
        if (!emptiablePallets.Any()) {
            return null;
        }
        var nextTrasnferSource = emptiablePallets.OrderBy(x => x.Quantity).First();
        var nextTransferDestination = shippingPallets.Where(shpPallet => shpPallet.NextHinban == nextTrasnferSource.Hinban).OrderBy(x => x.RemainStep).First();
        return new TransferDirection(nextTrasnferSource.Hinban, nextTrasnferSource.LocationCode, nextTransferDestination.LocationCode);
    }
    /// <summary>
    /// 仕掛パレットで使用しない在庫パレットの積替え判断
    /// 
    /// 以下の手順で積替え品番、積替え元、積み替え先を求める。
    /// 1. 仕掛パレット置き場から仕掛パレットの積み込み情報のリストを取得する
    /// 2. 取得した積み込み情報のリストに問い合わせ、仕掛パレットで使用しない在庫パレットのリストを取得する
    /// 
    /// 積み替え元の選択
    /// - 仕掛パレットで使用しない在庫パレットが複数ある場合は在庫数が少ないものを返す
    /// 積替え先の選択
    /// - 積替え先の品番が複数ある場合は積替えステップが最も小さいものを選択する
    /// </summary>
    /// <remarks>積替え先の品番が見つからない場合はnullを返す</remarks>
    private TransferDirection? FilterInventoryPalletsNotUsedInShikakariStorage(ShippingStationCode stationCode, IEnumerable<TemporaryStoragePalletInfo> pallets) {
        var shippingPallets = _shippingStorageLoader.GetLoadableFrom(stationCode, pallets);
        if (!shippingPallets.Any()) {
            return null;
        }
        var shikakariPallets = _ShikakariStorageLoader.GetLoadableFrom(pallets);
        var notUsedPallets = pallets.Where(tmpPallet => {
            return shikakariPallets.Any(shkPallet => shkPallet.IsLoadableQuantityGreaterThan(tmpPallet.Hinban, 0)) == false;
        });
        if (!notUsedPallets.Any()) {
            return null;
        }
        var nextTrasnferSource = notUsedPallets.OrderBy(x => x.Quantity).First();
        var nextTransferDestination = shippingPallets.Where(shpPallet => shpPallet.NextHinban == nextTrasnferSource.Hinban).OrderBy(x => x.RemainStep).First();
        return  new TransferDirection(nextTrasnferSource.Hinban, nextTrasnferSource.LocationCode, nextTransferDestination.LocationCode);
    }
    /// <summary>
    /// 積替え可能な回数が少ない出荷パレットを抽出するフィルタ
    /// 
    /// 以下の手順で積替え品番、積替え元、積み替え先を求める。
    /// 1. 一時置き場から出荷パレットの積み込み情報のリストを取得する
    /// 2. 取得した積み込み情報のリストに問い合わせ、積替え可能な回数が少ない出荷パレットのリストを取得する
    /// 
    /// 積み替え元の選択
    /// - 積替え先の品番にあわせる（同一品番が一時置き場に 2つ来ることはない）
    /// 積替え先の選択
    /// - 積替え可能な回数が同じパレットが複数ある場合は積替えステップが最も小さいものを選択する
    /// </summary>
    /// <remarks>
    /// 事前に空になる在庫パレットの判定が行われているため、積込対象の品番の数量は不足しない前提で判定を行う
    /// 積み替え可能な回数が同じパレットが複数ある場合、積替え元パレットの在庫数を基準に判定するほうが効率が
    /// 良いと思われるが、レアケースであるため同じデータソースで並べ替え可能な Step が最も小さいパレットを選択する
    /// 積替え先の品番が見つからない場合はnullを返す
    /// </remarks>
    private TransferDirection? FilterShippingPalletsWithFewTransfers(ShippingStationCode stationCode, IEnumerable<TemporaryStoragePalletInfo> pallets) {
        var shippingPallets = _shippingStorageLoader.GetLoadableFrom(stationCode, pallets);
        if (!shippingPallets.Any()) {
            return null;
        }
        var nextTransferDistination = shippingPallets.OrderBy(x => x.RequiredHinbanTypeCount).ThenBy(x => x.RemainStep).First();
        var nextTransferSource = pallets.First(x => x.Hinban == nextTransferDistination.NextHinban);
        return new TransferDirection(nextTransferDistination.NextHinban, nextTransferSource.LocationCode, nextTransferDistination.LocationCode);
    }
}
