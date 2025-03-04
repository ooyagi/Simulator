using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IPutonShippingPalletItemService
{
    void Puton(LocationCode locationCode, Hinban hinban);
}
