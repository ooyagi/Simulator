using System.ComponentModel.DataAnnotations;
using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

public class ShikakariStorage
{
    [Key]
    public LocationCode LocationCode { get; set; } = LocationCode.Default;
    public int Index { get; set; } = 0;
    public StorageStatus Status { get; set; } = StorageStatus.Empty;
    public ShippingPalletID? ShippingPalletID { get; set; }

    # nullable disable
    public ShippingPallet StoredPallet { get; set; } 
    # nullable enable

    public ShikakariStorage() { }
    public ShikakariStorage(
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
