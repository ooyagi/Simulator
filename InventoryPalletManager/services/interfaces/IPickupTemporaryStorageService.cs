using CommonItems.Models;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

interface IPickupTemporaryStorageService
{
    void Pickup(LocationCode locationCode, InventoryPalletID inventoryPalletID);
}
