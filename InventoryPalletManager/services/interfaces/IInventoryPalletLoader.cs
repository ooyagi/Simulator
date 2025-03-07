using CommonItems.Models;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

interface IInventoryPalletLoader
{
    InventoryPallet? Find(InventoryPalletID palletId);
    IEnumerable<InventoryPallet> FliterByHinban(Hinban hinban);
}
