using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

interface IStorableHinbanLoader
{
    bool IsStorable(Hinban hinban);
}
