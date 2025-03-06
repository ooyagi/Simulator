using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IWorkOrderLoader
{
    IEnumerable<ShippingPalletID> GetUsageWorkOrderIdByHinban(Hinban hinban);
}
