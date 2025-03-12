using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

class CheckStepToCompletion
{
    /// <summary>
    /// 出荷作業完了までのステップ数を返す
    /// 
    /// 指定された items で出荷完了出来ない場合は -1 を返す
    /// </summary>
    public static int GetStepToCompletion(IEnumerable<ShippingPalletItem> palletItems,  IEnumerable<ILoadableItem> loadableItems) {
        var step = 0;
        var remainingItems = palletItems.Where(x => !x.IsCompleted).ToList();
        var tmpItems = loadableItems.Select(x => new LocalLoadableItem(x.Hinban, x.Quantity)).ToList();
        foreach (var item in remainingItems) {
            var tmp = tmpItems.FirstOrDefault(x => x.Hinban == item.Hinban && 0 < x.Quantity);
            if (tmp == null) {
                return -1;
            }
            tmp = tmp with { Quantity = tmp.Quantity - 1 };
            step++;
        }
        return step;
    }
    record LocalLoadableItem(Hinban Hinban, int Quantity): ILoadableItem;
}
