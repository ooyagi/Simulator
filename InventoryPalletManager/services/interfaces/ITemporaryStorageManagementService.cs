using CommonItems.Models;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

public interface ITemporaryStorageManagementService
{
    TemporaryStorage Add(ShippingStationCode shippingStationCode, LocationCode locationCode);
    void Clear();
}
