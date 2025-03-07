using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

interface IPickupTemporaryStorageService
{
    InventoryPalletID? Pickup(LocationCode locationCode);
}
