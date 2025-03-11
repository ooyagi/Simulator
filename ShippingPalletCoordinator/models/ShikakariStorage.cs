using System.ComponentModel.DataAnnotations;
using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

public class ShikakariStorage
{
    [Key]
    public LocationCode LocationCode { get; set; } = LocationCode.Default;
    public StorageStatus Status { get; set; } = StorageStatus.Empty;
    public ShippingPalletID? ShippingPalletID { get; set; }

    # nullable disable
    public ShippingPallet StoredPallet { get; set; } 
    # nullable enable

    public ShikakariStorage() { }
    public ShikakariStorage(
        LocationCode locationCode
    ) {
        LocationCode = locationCode;
    }

    public ShippingPalletID? Pickup() {
        var tmp = ShippingPalletID;
        Status = StorageStatus.Empty;
        ShippingPalletID = null;
        return tmp;
    }
    public void Place(ShippingPalletID shippingPalletID) {
        Status = StorageStatus.InUse;
        ShippingPalletID = shippingPalletID;
    }
}
