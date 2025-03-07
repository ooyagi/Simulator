using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

interface IPlaceTemporaryStorageService
{
    void Place(LocationCode locationCode, InventoryPalletID inventoryPalletID);
}
