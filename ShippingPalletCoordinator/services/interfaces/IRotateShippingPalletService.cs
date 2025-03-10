using CommonItems.Models;

namespace ShippingPalletCoordinator.Services;

interface IRotateShippingPalletService
{
    void Rotate(ShippingPalletID shippingPalletID);
}
