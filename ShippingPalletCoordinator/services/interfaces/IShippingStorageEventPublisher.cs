using CommonItems.Models;

namespace ShippingPalletCoordinator.Services;

interface IShippingStorageEventPublisher
{
    void PublishPickupEvent(LocationCode locationCode);
}
