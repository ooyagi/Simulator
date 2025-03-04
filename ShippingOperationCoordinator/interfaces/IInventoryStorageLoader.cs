using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IInventoryStorageLoader
{
    IEnumerable<IStoragedHinban> GetStoragedItems();
    bool IsPickable(ShippingStationCode stationCode, Hinban hinban, int quantity = 1);
}

public interface IStoragedHinban
{
    LocationCode LocationCode { get; }
    Hinban Hinban { get; }
    int Quantity { get; }
}
