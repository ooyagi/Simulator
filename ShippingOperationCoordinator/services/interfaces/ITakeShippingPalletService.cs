using CommonItems.Models;

namespace ShippingOperationCoordinator.Services;

interface ITakeShippingPalletService
{
    void TakeInitialPallets(ShippingStationCode stationCode);
    public void Take(ShippingStationCode stationCode);
}
