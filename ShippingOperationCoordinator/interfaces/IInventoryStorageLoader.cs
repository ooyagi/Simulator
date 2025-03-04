using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IInventoryStorageLoader
{
    bool IsExists(Hinban hinban);
    bool IsUseup(Hinban hinban, int loadableCount);
    IEnumerable<IStoragedHinban> GetStoragedItems();
}

public interface IStoragedHinban
{
    LocationCode LocationCode { get; }
    Hinban Hinban { get; }
    int Quantity { get; }
}

