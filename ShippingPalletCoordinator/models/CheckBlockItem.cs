using System.Security.Cryptography.X509Certificates;
using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

class CheckBlockItem
{
    public static (Hinban? hinban, int count) GetBlockHinban(IEnumerable<ShippingPalletItem> palletItems, IEnumerable<ILoadableItem> loadableItems) {
        var remainingItems = palletItems.Where(x => !x.IsCompleted).ToList();
        var tmpItems = loadableItems.Select(x => new LocalLoadableItem(x.Hinban, x.Quantity)).ToList();
        Hinban? blockHinban = null;
        int blockHinbanLoadableCount = 0;
        foreach (var item in remainingItems) {
            var tmp = tmpItems.FirstOrDefault(x => x.Hinban == item.Hinban && 0 < x.Quantity);
            if (tmp != null) {                          // 積み込み対象に必要品番がある場合は積み込み対象の在庫数を減らす
                tmp = tmp with { Quantity = tmp.Quantity - 1 };
            } else if (tmp == null && blockHinban == null) {   // 積み込み対象に必要品番が無く、ブロック品番が未設定の場合はブロック品番を設定
                blockHinban = item.Hinban;
                blockHinbanLoadableCount = blockHinbanLoadableCount + 1;
            } else if (blockHinban == item.Hinban) {    // 必要品番がブロック品番と一致する場合は積み込み可能数をカウント
                blockHinbanLoadableCount = blockHinbanLoadableCount + 1;
                continue;
            } else {
                break;
            }
        }
        return (blockHinban, blockHinbanLoadableCount);
    }
    record LocalLoadableItem(Hinban Hinban, int Quantity): ILoadableItem;
}
