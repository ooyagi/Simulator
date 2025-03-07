using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

interface IPickupInventoryStorageService
{
    InventoryPalletID? Pickup(LocationCode locationCode);
}
