using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface ITakeShippingPalletService
{
    void Request(LocationCode locationCode, ShippingPalletID palletId);
}
