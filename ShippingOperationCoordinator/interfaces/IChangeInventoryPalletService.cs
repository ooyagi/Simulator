using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IChangeInventoryPalletService
{
    bool Change(ShippingStationCode stationCode);
    bool ChangeEmptyPallet(ShippingStationCode stationCode);
}
