using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IChangeShippingPalletService
{
    bool Change(ShippingStationCode stationCode);
}
