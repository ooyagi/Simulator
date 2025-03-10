using CommonItems.Models;

namespace ShippingPalletCoordinator.Services;

interface IPickupShikakariStorageService
{
    ShippingPalletID? Pickup(LocationCode locationCode);
}
