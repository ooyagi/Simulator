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

    // ReserveをLoaderに入れるのは変だが試算かつ最後の調整なのでここに入れてしまう。
    ShikakariStorage? ReserveEmptyLocation();
}
