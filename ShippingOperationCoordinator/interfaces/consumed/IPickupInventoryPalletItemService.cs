using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IPickupInventoryPalletItemService
{
    IPickupItemResult? Pickup(LocationCode locationCode);
}

public interface IPickupItemResult
{
    Hinban Hinban { get; }
}
