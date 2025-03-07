using CommonItems.Models;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

interface IInventoryStorageLoader
{
    InventoryStorage? Find(LocationCode locationCode);
    LocationCode? FindEmptyLocation();
}

interface IInventoryStorageInfo
{
    LocationCode LocationCode { get; }
    StorageStatus Status { get; }
    InventoryPalletID? InventoryPalletID { get; }
}
