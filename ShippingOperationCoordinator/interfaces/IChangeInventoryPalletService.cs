using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IChangeInventoryPalletService
{
    void Change(ShippingStationCode stationCode);
    bool ChangeEmptyPallet(ShippingStationCode stationCode);
}
