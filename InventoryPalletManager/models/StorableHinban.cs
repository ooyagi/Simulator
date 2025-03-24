using System.ComponentModel.DataAnnotations;
using CommonItems.Models;

namespace InventoryPalletCoordinator.Models;

public class StorableHinban
{
    [Key]
    public Hinban Hinban { get; } = Hinban.Default;
    public int Level { get; set; } = 0;

    public StorableHinban() { }
    public StorableHinban(Hinban hinban) {
        Hinban = hinban;
    }
}
