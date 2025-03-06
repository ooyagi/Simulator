using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IInventoryStorageLoader
{
    bool IsExists(Hinban hinban);
    bool IsUseup(Hinban hinban, int loadableCount);
    IEnumerable<IInventoryPalletInfo> GetStoragedItems();
}
