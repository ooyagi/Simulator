using System.ComponentModel.DataAnnotations;
using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

public class ShippingStorage
{
    [Key]
    public LocationCode LocationCode { get; set; } = LocationCode.Default;
    public ShippingStationCode ShippingStationCode { get; set; } = ShippingStationCode.Default;
    public int Index { get; set; } = 0;
    public StorageStatus Status { get; set; } = StorageStatus.Empty;
    public ShippingPalletID? ShippingPalletID { get; set; }

    # nullable disable
    public ShippingPallet StoredPallet { get; set; } 
    # nullable enable

    public ShippingStorage() { }
    public ShippingStorage(
        LocationCode locationCode,
        int index
    ) {
        LocationCode = locationCode;
        Index = index;
    }

    public void Place(ShippingPalletID shippingPalletID) {
        Status = StorageStatus.InUse;
        ShippingPalletID = shippingPalletID;
    }
}
