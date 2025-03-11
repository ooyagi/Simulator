using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

interface IShikakariStorageLoader
{
    ShikakariStorage? Find(LocationCode locationCode);
    ShikakariStorage? FindEmptyLocation();
    IEnumerable<ShikakariStorage> GetEmptyLocations();
    LocationCode? FindStoredLocation(ShippingPalletID palletId);
}
