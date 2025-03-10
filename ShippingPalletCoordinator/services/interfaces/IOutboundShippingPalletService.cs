using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

interface IOutboundShippingPalletService
{
    void Outbound(ShippingPalletID shippingPalletId);
}
