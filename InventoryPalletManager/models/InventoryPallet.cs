using System.ComponentModel.DataAnnotations;
using CommonItems.Models;

namespace InventoryPalletCoordinator.Models;

public class InventoryPallet
{
    [Key]
    public InventoryPalletID Id { get; set; }
    public Hinban Hinban { get; set; } = Hinban.Default;
    public int Quantity { get; set; } = 0;

    private static int FULL_QUANTITY = 14;

    public InventoryPallet() {
        Id = CreateId();
    }
    public InventoryPallet(
        Hinban hinban
    ) {
        Id = CreateId();
        Hinban = hinban;
        Quantity = FULL_QUANTITY;
    }

    public InventoryPalletID CreateId() {
        return new InventoryPalletID(Guid.NewGuid().ToString());
    }
    public void PickupItem() {
        if (Quantity == 0) {
            throw new InvalidOperationException("The quantity of the pallet is 0.");
        }
        Quantity--;
    }
}
