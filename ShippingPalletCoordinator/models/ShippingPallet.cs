using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

public class ShippingPallet
{
    [Key]
    public ShippingPalletID Id { get; set; } = ShippingPalletID.CustomPaletteID;
    public int Priority { get; set; } = 0;      // 内部での処理順・入力情報の優先度をそのまま反映しない（試算時のみか？）
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
        int priority,
        IEnumerable<ShippingPalletItem> items
    ) {
        Id = id;
        Priority = priority;
        Items = items.OrderBy(x => x.Index).ToList();
    }

    /// <summary>
    /// 出荷作業完了までのステップ数を返す
    /// 
    /// 指定された items で出荷完了出来ない場合は -1 を返す
    /// </summary>
    internal int GetStepToCompletion(IEnumerable<ILoadableItem> items) {
        return CheckStepToCompletion.GetStepToCompletion(Items, items);
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
}

internal interface ILoadableItem {
    Hinban Hinban { get; }
    int Quantity { get; }
}
