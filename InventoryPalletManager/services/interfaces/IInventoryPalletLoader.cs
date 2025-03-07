using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

interface IInventoryPalletLoader
{
    InventoryPallet? Find(InventoryPalletID palletId);
}
