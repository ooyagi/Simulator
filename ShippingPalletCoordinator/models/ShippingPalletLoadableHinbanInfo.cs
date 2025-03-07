using CommonItems.Models;

namespace ShippingPalletCoordinator.Models;

public class ShippingPalletLoadableHinbanInfo: ShippingOperationCoordinator.Interfaces.IShippingPalletLoadableHinbanInfo, ShippingOperationCoordinator.Interfaces.IShikakariPalletLoadableHinbanInfo
{
    public LocationCode LocationCode { get; set; }
    public ShippingPalletID ShippingPalletID { get; set; }
    public Hinban NextHinban => throw new NotImplementedException();
    public Hinban BlockHinban => throw new NotImplementedException();
    public int RemainStep => throw new NotImplementedException();
    public int FutureLoadableHinbanTypeCount => throw new NotImplementedException();

    public int BlockHinbanLoadableCount => throw new NotImplementedException();

    public ShippingPalletLoadableHinbanInfo(
        LocationCode locationCode,
        ShippingPallet shippingPallet
    ) {
        LocationCode = locationCode;
        ShippingPalletID = shippingPallet.Id;
    }

    public bool IsCompletableBy(ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo pickableItem) {
        throw new NotImplementedException();
    }
    public bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity) {
        throw new NotImplementedException();
    }
    public IEnumerable<ShippingOperationCoordinator.Interfaces.IEmptiablePalletInfo> GetEmptiablePallets(IEnumerable<ShippingOperationCoordinator.Interfaces.IInventoryPalletInfo> inventoryPallets)
    {
        throw new NotImplementedException();
    }
}
