using CommonItems.Models;
using ShippingOperationCoordinator.Models;

namespace ShippingOperationCoordinator.Services;

public interface IShippingStationManagementService
{
    ShippingStation Add(ShippingStationCode shippingStationCode);
    void Clear();
}
