using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface ITakePickupInventoryPalletService
{
    void Request(LocationCode locationCode, Hinban hinban);
}
