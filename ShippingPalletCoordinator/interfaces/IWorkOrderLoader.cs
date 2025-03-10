using CommonItems.Models;

namespace ShippingPalletCoordinator.Interfaces;

public interface IWorkOrderLoader
{
    IWorkOrder GetNextOrder();
}
public interface IWorkOrder
{
    ShippingPalletID ShippingPalletID { get; }
}
