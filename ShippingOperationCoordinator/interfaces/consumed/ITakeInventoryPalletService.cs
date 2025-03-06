using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface ITakeInventoryPalletService
{
    void Request(LocationCode locationCode, Hinban hinban);
}
