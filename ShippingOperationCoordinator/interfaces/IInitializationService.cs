namespace ShippingOperationCoordinator.Interfaces;

public interface IInitializationService
{
    void TakeInitialShippingPallets();
    void TakeInitialInventoryPallets();
    void Initialize();
}
