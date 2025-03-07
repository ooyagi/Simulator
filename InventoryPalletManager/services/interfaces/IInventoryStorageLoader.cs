using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

interface IInventoryStorageLoader
{
    LocationCode? FindEmptyLocation();
}
