using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

interface IShippingStorageLoader
{
    int GetLastIndex(ShippingStationCode shippingStationCode);
    ShippingStationCode? ConvertStationCode(LocationCode locationCode);
    ShippingStorage? Find(LocationCode locationCode);
    IEnumerable<ShippingStorage> GetEmptyLocations();
}
