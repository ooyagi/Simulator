using System.ComponentModel.DataAnnotations;
using CommonItems.Models;

namespace InventoryPalletCoordinator.Models;

public class InventoryPallet
{
    [Key]
    public InventoryPalletID Id { get; }
    public Hinban Hinban { get; } = Hinban.Default;
    public int Quantity { get; } = 0;

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
}
