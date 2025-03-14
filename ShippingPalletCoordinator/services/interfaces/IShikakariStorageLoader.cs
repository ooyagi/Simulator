using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

interface IShikakariStorageLoader
{
    IEnumerable<ShikakariStorage> All();
    ShikakariStorage? Find(LocationCode locationCode);
    ShikakariStorage? FindEmptyLocation();
    IEnumerable<ShikakariStorage> GetEmptyLocations();
    LocationCode? FindStoredLocation(ShippingPalletID palletId);
}
