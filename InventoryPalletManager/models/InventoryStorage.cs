using System.ComponentModel.DataAnnotations;
using CommonItems.Models;
using InventoryPalletCoordinator.Services;

namespace InventoryPalletCoordinator.Models;

public class InventoryStorage: IInventoryStorageInfo
{
    [Key]
    public LocationCode LocationCode { get; set; } = LocationCode.Default;
    public StorageStatus Status { get; set; } = StorageStatus.Empty;
    public InventoryPalletID? InventoryPalletID { get; set; }

    public InventoryStorage() { }

    public void Place(InventoryPalletID inventoryPalletID) {
        Status = StorageStatus.InUse;
        InventoryPalletID = inventoryPalletID;
    }
}
