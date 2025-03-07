using System.ComponentModel.DataAnnotations;
using CommonItems.Models;
using InventoryPalletCoordinator.Services;

namespace InventoryPalletCoordinator.Models;

public class InventoryStorage: IInventoryStorageInfo
{
    [Key]
    public LocationCode LocationCode { get; set; } = LocationCode.Default;
    public int Index { get; set; } = 0;
    public StorageStatus Status { get; set; } = StorageStatus.Empty;
    public InventoryPalletID? InventoryPalletID { get; set; }

    # nullable disable
    public InventoryPallet StoredPallet { get; set; } 
    # nullable enable

    public Hinban? Hinban { get { return StoredPallet?.Hinban; } }

    public InventoryStorage() { }
    public InventoryStorage(
        LocationCode locationCode,
        int index
    ) {
        LocationCode = locationCode;
        Index = index;
    }

    public InventoryPalletID? Pickup() {
        Status = StorageStatus.Empty;
        var tmp = InventoryPalletID;
        InventoryPalletID = null;
        return tmp;
    }
    public void Place(InventoryPalletID inventoryPalletID) {
        Status = StorageStatus.InUse;
        InventoryPalletID = inventoryPalletID;
    }
}
