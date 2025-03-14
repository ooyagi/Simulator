using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IChangeShippingPalletService
{
    bool TakeInEmptyLocation(ShippingStationCode stationCode);
    bool Change(ShippingStationCode stationCode);
}
