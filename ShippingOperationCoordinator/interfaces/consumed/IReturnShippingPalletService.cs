using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IReturnShippingPalletService
{
    void Request(LocationCode locationCode);
}
