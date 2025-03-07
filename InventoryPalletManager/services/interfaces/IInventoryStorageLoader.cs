using CommonItems.Models;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

interface IInventoryStorageLoader
{
    InventoryStorage? Find(LocationCode locationCode);
    InventoryStorage? FindEmptyLocation();
    LocationCode? FindStoredLocation(InventoryPalletID inventoryPalletID);
    int GetLastIndex();
}

interface IInventoryStorageInfo
{
    LocationCode LocationCode { get; }
    StorageStatus Status { get; }
    InventoryPalletID? InventoryPalletID { get; }
}
