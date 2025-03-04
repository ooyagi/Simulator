using CommonItems.Models;

namespace ShippingOperationCoordinator.Services;

interface ITakeShippingPalletSelector
{
    ShippingPalletID? SelectTakeShippingPallet(ShippingStationCode stationCode);
}
