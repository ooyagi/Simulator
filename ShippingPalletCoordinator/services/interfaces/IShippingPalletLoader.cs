using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

interface IShippingPalletLoader
{
    ShippingPallet? Find(ShippingPalletID shippingPalletID);
}
