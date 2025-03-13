using CommonItems.Models;

namespace ShippingOperationCoordinator.Services;

interface ITakeShippingPalletSelector
{
    IEnumerable<(LocationCode, ShippingPalletID)> SelectInitialShippingPallet(ShippingStationCode stationCode, IEnumerable<LocationCode> emptyLocations);
    ShippingPalletID? SelectTakeShippingPallet(ShippingStationCode stationCode);
}
