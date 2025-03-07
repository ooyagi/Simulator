using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

interface ITransportRequestService
{
    void Request(TransportType transportType, LocationCode from, LocationCode to);
}
