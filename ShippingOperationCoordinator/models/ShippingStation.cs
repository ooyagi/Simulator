using CommonItems.Models;
using System.ComponentModel.DataAnnotations;

namespace ShippingOperationCoordinator.Models;

public class ShippingStation
{
    [Key]
    public ShippingStationCode Code { get; } = ShippingStationCode.Default;

    public ShippingStation() { }
    public ShippingStation(
        ShippingStationCode code
    ) {
        Code = code;
    }
}
