using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IReturnInventoryPalletService
{
    void Request(LocationCode locationCode);
}
