using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

public class ShippingPalletItem
{
    public ShippingPalletID PalletID { get; set; } = ShippingPalletID.CustomPaletteID;
    public Hinban Hinban { get; set; } = Hinban.Default;
    public int Index { get; set; }
    public bool IsCompleted { get; set; }

    public ShippingPalletItem() { }
    public ShippingPalletItem(
        ShippingPalletID palletID,
        Hinban hinban,
        int index
    ) {
        PalletID = palletID;
        Hinban = hinban;
        Index = index;
    }

    public void Complete() {
        IsCompleted = true;
    }
}
