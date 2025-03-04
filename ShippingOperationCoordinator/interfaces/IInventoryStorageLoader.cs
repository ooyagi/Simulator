using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IInventoryStorageLoader
{
    IEnumerable<IStoragedHinban> GetStoragedItems();
}

public interface IStoragedHinban
{
    LocationCode LocationCode { get; }
    Hinban Hinban { get; }
    int Quantity { get; }
}
