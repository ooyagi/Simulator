using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

public interface IShippingStorageManagementService
{
    ShippingStorage Add(ShippingStationCode shippingStationCode, LocationCode locationCode);
    void Clear();
}
