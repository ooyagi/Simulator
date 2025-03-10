using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

interface IShikakariStorageLoader
{
    ShikakariStorage? FindEmptyLocation();
    LocationCode? FindStoredLocation(ShippingPalletID palletId);
}
