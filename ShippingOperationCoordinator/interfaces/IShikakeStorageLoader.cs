using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IShikakariStorageLoader
{
    IEnumerable<IShikakariPalletLoadableHinbanInfo> GetLoadableFrom(IEnumerable<ITransferablePalletInfo> pallets);
}

public interface IShikakariPalletLoadableHinbanInfo
{
    LocationCode LocationCode { get; }
    ShippingPalletID ShippingPalletID { get; }
    Hinban NextHinban { get; }
    int Step { get; }

    bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity);
}
