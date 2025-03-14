using System.ComponentModel.DataAnnotations;
using CommonItems.Models;

namespace InventoryPalletCoordinator.Models;

public class StorableHinban
{
    [Key]
    public Hinban Hinban { get; } = Hinban.Default;

    public StorableHinban() { }
    public StorableHinban(Hinban hinban) {
        Hinban = hinban;
    }
}
