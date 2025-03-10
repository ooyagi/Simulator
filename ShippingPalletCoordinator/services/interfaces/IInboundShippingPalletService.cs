using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

interface IInboundShippingPalletService
{
    void Inbound(ShippingPallet shippingPallet);
}
