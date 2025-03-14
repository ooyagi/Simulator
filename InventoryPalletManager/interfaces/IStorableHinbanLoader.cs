using CommonItems.Models;

namespace InventoryPalletCoordinator.Interfaces;

public interface IStorableHinbanLoader
{
    IEnumerable<Hinban> GetHinbans();
}
