using CommonItems.Models;

namespace ShippingOperationCoordinator.Services;

interface ITakeInventoryPalletSelector
{
    Hinban? SelectTakeInventoryPallet(ShippingStationCode stationCode);
}
