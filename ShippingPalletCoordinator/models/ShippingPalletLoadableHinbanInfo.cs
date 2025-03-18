using CommonItems.Models;
using Microsoft.VisualBasic;

namespace ShippingPalletCoordinator.Models;

/// <summary>
/// 出荷パレットに積み込むことが出来る品番情報
/// 
/// _items には出荷パレットに積み込むことが出来る品番情報が格納されている
/// </summary>
class ShippingPalletLoadableHinbanInfo: ShippingOperationCoordinator.Interfaces.IShippingPalletLoadableHinbanInfo, ShippingOperationCoordinator.Interfaces.IShikakariPalletLoadableHinbanInfo, ShippingOperationCoordinator.Interfaces.IShippingPalletLoadableHinbanInfoNoLocation
{
    public LocationCode LocationCode { get; set; }
    public ShippingPalletID ShippingPalletID { get; set; }
    public Hinban? NextHinban => _items.FirstOrDefault(x => !x.IsCompleted)?.Hinban;
    public Hinban? BlockHinban => _blockHinban;
    public int RemainStep => _items.Count(x => !x.IsCompleted);
    public int RequiredHinbanTypeCount => _items.Where(x => !x.IsCompleted).Select(x => x.Hinban).Distinct().Count();
    public int BlockHinbanLoadableCount => _blockHinbanLoadableCount;
    public bool IsLoadable => _loadableItems.Any(x => x.Hinban == NextHinban);

    List<ShippingPalletItem> _items;
    List<LocalLoadableItem> _loadableItems;
    Hinban? _blockHinban;
    int _blockHinbanLoadableCount;

    public ShippingPalletLoadableHinbanInfo(
        LocationCode locationCode,
        ShippingPallet shippingPallet,
        IEnumerable<ILoadableItem> loadableItems
    ) {
        LocationCode = locationCode;
        ShippingPalletID = shippingPallet.Id;
        _items = shippingPallet.Items.Select(x => new ShippingPalletItem(x.PalletID, x.Hinban, x.Index, x.IsCompleted)).ToList();
        _loadableItems = loadableItems.Select(x => new LocalLoadableItem(x.Hinban, x.Quantity)).ToList();
        (_blockHinban, _blockHinbanLoadableCount) = CheckBlockItem.GetBlockHinban(_items, _loadableItems);
    }

    /// <summary>
    /// 指定された品番を積み込むことで出荷完了するか確認する
    /// </summary>
    /// <remarks>
    /// オブジェクト作成時の loadableItems + 今回の入力で完了するかどうかを確認する
    /// </remarks>
    public bool IsCompletableBy(ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo pickableItem) {
        // 次の品番が異なる場合は出荷完了出来ない・出荷までに必要な品番が 2種以上の場合は出荷完了出来ない
        if (NextHinban != pickableItem.Hinban || 1 < RequiredHinbanTypeCount) {
            return false;
        }
        // ここまでの判定で出荷完了までに積み込みが必要な品番は NextHinban のみであることが
        // 確定しているため、残りステップ数が在庫数以上の場合は出荷完了可能
        return RemainStep <= pickableItem.Quantity;
    }
    /// <summary>
    /// 指定された品番を指定数以上積み込むことが出来るか確認する
    /// </summary>
    public bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity) {
        var loadableCount = CheckLoadableQuantity.GetLoadableQuantity(_items, _loadableItems, hinban);
        return quantity <= loadableCount;
    }

    /// <summary>
    /// 入力された在庫パレットの内、積み込みによって使い切れるパレットのリストを返す。
    /// 
    /// 使い切れる場合には使い切りまでのSTEP数を返す
    /// </summary>
    public IEnumerable<ShippingOperationCoordinator.Interfaces.IEmptiablePalletInfo> GetEmptiablePallets(IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> inventoryPallets) {
        IEnumerable<(Hinban, int)> emptablePalletInfos = CheckLoadableQuantity.GetEmptiablePallets(_items, _loadableItems);
        return inventoryPallets
            .Where(x => emptablePalletInfos.Any(y => y.Item1 == x.Hinban))
            .Select(x => new EmptiablePalletInfo(x.LocationCode, x.Hinban, emptablePalletInfos.First(y => y.Item1 == x.Hinban).Item2));
    }

    record LocalLoadableItem(Hinban Hinban, int Quantity): ILoadableItem;
    record EmptiablePalletInfo(LocationCode LocationCode, Hinban Hinban, int EmptiableStep): ShippingOperationCoordinator.Interfaces.IEmptiablePalletInfo;
}
