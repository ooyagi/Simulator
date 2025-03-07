using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

public class ShippingPallet
{
    [Key]
    public ShippingPalletID Id { get; set; } = ShippingPalletID.CustomPaletteID;

    [NotMapped]
    public Hinban NextHinban { get { throw new NotImplementedException(); } }
    [NotMapped]
    public bool IsCompleted { get { throw new NotImplementedException(); } }

    /// <summary>
    /// 出荷作業完了までのステップ数を返す
    /// 
    /// 指定された items で出荷完了出来ない場合は -1 を返す
    /// </summary>
    internal int GetStepToCompletion(IEnumerable<ILoadableItem> items) {
        throw new NotImplementedException();
    }

    public void PutonItem(Hinban hinban) {
        throw new NotImplementedException();
    }
}

internal interface ILoadableItem {
    Hinban Hinban { get; }
    int Quantity { get; }
}
