using CommonItems.Models;

namespace ShippingOperationCoordinator.Services;

interface ITakeInventoryPalletService
{
    public void Take(ShippingStationCode stationCode);
}
