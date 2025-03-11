using System.ComponentModel.DataAnnotations;
using CommonItems.Models;
using InventoryPalletCoordinator.Services;

namespace InventoryPalletCoordinator.Models;

public class TemporaryStorage: ITemporaryStorageInfo
{
    [Key]
    public LocationCode LocationCode { get; set; } = LocationCode.Default;
    public ShippingStationCode ShippingStationCode { get; set; } = ShippingStationCode.Default;
    public StorageStatus Status { get; set; } = StorageStatus.Empty;
    public InventoryPalletID? InventoryPalletID { get; set; }

    # nullable disable
    public InventoryPallet StoredPallet { get; set; } 
    # nullable enable

    public Hinban? Hinban { get { return StoredPallet?.Hinban; } }

    public TemporaryStorage() { }
    public TemporaryStorage(
        ShippingStationCode shippingStationCode,
        LocationCode locationCode
    ) {
        ShippingStationCode = shippingStationCode;
        LocationCode = locationCode;
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
