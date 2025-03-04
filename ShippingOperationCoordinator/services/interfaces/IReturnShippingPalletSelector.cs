using CommonItems.Models;

namespace ShippingOperationCoordinator.Services;

interface IReturnShippingPalletSelector
{
    LocationCode? SelectReturnShippingPallet(ShippingStationCode stationCode);
}
