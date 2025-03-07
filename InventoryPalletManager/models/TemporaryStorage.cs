using System.ComponentModel.DataAnnotations;
using CommonItems.Models;
using InventoryPalletCoordinator.Services;

namespace InventoryPalletCoordinator.Models;

public class TemporaryStorage: ITemporaryStorageInfo
{
    [Key]
    public LocationCode LocationCode { get; set; } = LocationCode.Default;
    public StorageStatus Status { get; set; } = StorageStatus.Empty;
    public InventoryPalletID? InventoryPalletID { get; set; }

    # nullable disable
    public InventoryPallet StoredPallet { get; set; } 
    # nullable enable

    public TemporaryStorage() { }

    public void Pickup() {
        Status = StorageStatus.Empty;
        InventoryPalletID = null;
    }
}
