using CommonItems.Models;

namespace WorkOrderManagement.Services;

public class WorkOrderLoader: ShippingOperationCoordinator.Interfaces.IWorkOrderLoader
{
    public IEnumerable<ShippingPalletID> GetUsageWorkOrderIdByHinban(Hinban hinban) {
        throw new NotImplementedException();
    }
}
