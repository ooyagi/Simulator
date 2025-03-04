using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IReturnPickupInventoryPalletService
{
    void Request(LocationCode locationCode);
}
