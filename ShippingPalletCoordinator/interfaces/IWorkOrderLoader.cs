using CommonItems.Models;

namespace ShippingPalletCoordinator.Interfaces;

public interface IWorkOrderLoader
{
    IWorkOrder? GetNextOrder();
}
public interface IWorkOrder
{
    ShippingPalletID ShippingPalletID { get; }
    IEnumerable<IWorkItem> WorkItems { get; }
}
public interface IWorkItem
{
    Hinban Hinban { get; }
    int Index { get; }
}
