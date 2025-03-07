using CommonItems.Models;

namespace ShippingPalletCoordinator.Services;

interface ITransportRequestService
{
    void Request(TransportType transportType, LocationCode from, LocationCode to);
}
