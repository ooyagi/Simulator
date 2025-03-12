using CommonItems.Models;
using ShippingOperationCoordinator.Models;

namespace ShippingOperationCoordinator.Services;

interface IDetermineTransferItemService
{
    TransferDirection? DetermineTransferHinban(ShippingStationCode stationCode);
}
