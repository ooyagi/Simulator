using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IReturnPickupShippingPalletService
{
    void Request(LocationCode locationCode);
}
