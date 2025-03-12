using System.Data;
using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

class CheckLoadableQuantity
{
    /// <summary>
    /// 指定された品番の積み込み可能数を取得する
    /// </summary>
    public static int GetLoadableQuantity(IEnumerable<ShippingPalletItem> palletItems, IEnumerable<ILoadableItem> loadableItems, Hinban hinban) {
        var count = 0;
        var remainingItems = palletItems.Where(x => !x.IsCompleted).ToList();
        var tmpItems = loadableItems.Select(x => new LocalLoadableItem(x.Hinban, x.Quantity)).ToList();
        foreach (var item in remainingItems) {
            var tmp = tmpItems.FirstOrDefault(x => x.Hinban == item.Hinban && 0 < x.Quantity);
            if (tmp == null) {
                return count;
            }
            tmp = tmp with { Quantity = tmp.Quantity - 1 };
            if (item.Hinban == hinban) {
                count++;
            }
        }
        return count;
    }
    /// <summary>
    /// 入力された在庫パレットの内、積み込みによって使い切れるパレットのリストを返す。
    /// 
    /// 使い切れる場合は使い切りまでのステップ数を返す
    /// </summary>
    public static IEnumerable<(Hinban, int)> GetEmptiablePallets(IEnumerable<ShippingPalletItem> palletItems, IEnumerable<ILoadableItem> loadableItems) {
        var remainingItems = palletItems.Where(x => !x.IsCompleted).ToList();
        var tmpItems = loadableItems.Select(x => new LocalLoadableItem(x.Hinban, x.Quantity)).ToList();

        // 積み込みが行えなくなるまで、品番毎の利用数と最終積み込みステップ数を記録
        int lastStep = 0;
        var usageInfo = loadableItems.ToDictionary(x => x.Hinban, x => new UsageInfo(0, 0));
        foreach (var item in remainingItems) {
            var tmp = tmpItems.FirstOrDefault(x => x.Hinban == item.Hinban && 0 < x.Quantity);
            if (tmp == null) {
                break;
            }
            lastStep++;
            tmp = tmp with { Quantity = tmp.Quantity - 1 };
            usageInfo[item.Hinban] = usageInfo[item.Hinban] with { count = usageInfo[item.Hinban].count + 1, lastStep = lastStep };
        }
        // 積み込み可能数が在庫数以上の在庫パレットの品番と最終積み込みステップ数を返す
        return usageInfo.Where(x => {
            var loadableItem = tmpItems.FirstOrDefault(y => y.Hinban == x.Key);
            return loadableItem != null && loadableItem.Quantity <= x.Value.count;
        }).Select(x => (x.Key, x.Value.lastStep));
    }
    record UsageInfo(int count, int lastStep);
    record LocalLoadableItem(Hinban Hinban, int Quantity): ILoadableItem;
}
