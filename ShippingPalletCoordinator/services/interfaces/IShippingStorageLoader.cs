using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

interface IShippingStorageLoader
{
    ShippingStationCode? ConvertStationCode(LocationCode locationCode);
    ShippingStorage? Find(LocationCode locationCode);
}
