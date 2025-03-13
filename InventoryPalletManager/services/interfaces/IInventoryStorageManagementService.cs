using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

public interface IInventoryStorageManagementService
{
    InventoryStorage AddAuto();
    void Clear();
}
