using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

public class ShippingPallet
{
    [Key]
    public ShippingPalletID Id { get; set; } = ShippingPalletID.CustomPaletteID;
    public IEnumerable<ShippingPalletItem> Items { get; set; } = new List<ShippingPalletItem>();

    [NotMapped]
    public Hinban? NextHinban { get { return NextItem?.Hinban; } }
    [NotMapped]
    public ShippingPalletItem? NextItem { get { return Items.Where(x => !x.IsCompleted).FirstOrDefault(); } }
    [NotMapped]
    public bool IsCompleted { get { return Items.All(x => x.IsCompleted); } }

    public ShippingPallet() { }
    public ShippingPallet(
        ShippingPalletID id,
        IEnumerable<ShippingPalletItem> items
    ) {
        Id = id;
        Items = items.OrderBy(x => x.Index).ToList();
    }

    /// <summary>
    /// 出荷作業完了までのステップ数を返す
    /// 
    /// 指定された items で出荷完了出来ない場合は -1 を返す
    /// </summary>
    internal int GetStepToCompletion(IEnumerable<ILoadableItem> items) {
        var step = 0;
        var remainingItems = Items.Where(x => !x.IsCompleted).ToList();
        var tmpItems = items.Select(x => new LocalLoadableItem(x.Hinban, x.Quantity)).ToList();
        foreach (var item in remainingItems) {
            var tmp = tmpItems.FirstOrDefault(x => x.Hinban == item.Hinban && 0 < x.Quantity);
            if (tmp == null) {
                return -1;
            }
            tmp = tmp with { Quantity = tmp.Quantity - 1 };
            if (tmp.Quantity == 0) {
                tmpItems.Remove(tmp);
            }
            step++;
        }
        return step;
    }
    /// <summary>
    /// 指定された品番を積み込む
    /// </summary>
    public void PutonItem(Hinban hinban) {
        if (NextItem == null) {
            throw new InvalidOperationException("積み込み可能な品番がありません");
        }
        if (NextHinban != hinban) {
            throw new InvalidOperationException("指定された品番は積み込み可能な品番ではありません");
        }
        NextItem.Complete();
    }
    record LocalLoadableItem(Hinban Hinban, int Quantity): ILoadableItem;
}

internal interface ILoadableItem {
    Hinban Hinban { get; }
    int Quantity { get; }
}
