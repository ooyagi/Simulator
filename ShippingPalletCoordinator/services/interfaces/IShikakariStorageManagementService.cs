using CommonItems.Models;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

public interface IShikakariStorageManagementService
{
    ShikakariStorage Add(LocationCode locationCode);
    void Clear();
}
