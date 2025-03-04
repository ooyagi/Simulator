using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface ITakePickupShippingPalletService
{
    void Request(LocationCode locationCode, Hinban hinban);
}
