using CommonItems.Models;

namespace ShippingPalletCoordinator.Services;

interface IPickupShippingStorageService
{
    ShippingPalletID? Pickup(LocationCode locationCode);
}
