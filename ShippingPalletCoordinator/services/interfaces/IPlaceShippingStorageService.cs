using CommonItems.Models;

namespace ShippingPalletCoordinator.Services;

interface IPlaceShippingStorageService
{
    void Place(LocationCode locationCode, ShippingPalletID shippingPalletID);
}
