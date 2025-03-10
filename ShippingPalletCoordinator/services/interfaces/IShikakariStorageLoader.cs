using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

interface IShikakariStorageLoader
{
    ShikakariStorage? Find(LocationCode locationCode);
    ShikakariStorage? FindEmptyLocation();
    LocationCode? FindStoredLocation(ShippingPalletID palletId);
}
