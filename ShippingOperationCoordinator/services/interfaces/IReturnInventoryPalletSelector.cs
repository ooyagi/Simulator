using CommonItems.Models;

namespace ShippingOperationCoordinator.Services;

interface IReturnInventoryPalletSelector
{
    LocationCode? SelectReturnInventoryPallet(ShippingStationCode stationCode);
    LocationCode? SelectEmptyInventoryPallet(ShippingStationCode stationCode);
}
