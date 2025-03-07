using CommonItems.Models;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

interface IPlaceInventoryStorageService
{
    void Place(LocationCode locationCode, InventoryPalletID inventoryPalletID);
}
