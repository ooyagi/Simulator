using CommonItems.Models;

namespace ShippingPalletCoordinator.Services;

interface IPlaceShikakariStorageService
{
    void Place(LocationCode locationCode, ShippingPalletID inventoryPalletID);
}
