using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class ShippingPalletLoader: Services.IShippingPalletLoader, ShippingOperationCoordinator.Interfaces.IShippingPalletLoader
{
    private readonly IShippingPalletCoordinatorDbContext _context;

    public ShippingPalletLoader(
        IShippingPalletCoordinatorDbContext context
    ) {
        _context = context;
    }

    public ShippingPallet? Find(ShippingPalletID shippingPalletID) {
        return _context.ShippingPallets.FirstOrDefault(x => x.Id == shippingPalletID);
    }
    public IEnumerable<IShippingPalletLoadableHinbanInfoNoLocation> GetLoadableFrom(IEnumerable<IInventoryPalletInfo> pallets) {
        return _context.ShippingPallets.Select(x => new ShippingPalletLoadableHinbanInfo(
                LocationCode.Default,
                x,
                pallets.Select(y => new LocalLoadableItem(y.Hinban, y.Quantity)
            ).ToList()));
    }
    record LocalLoadableItem(Hinban Hinban, int Quantity): ILoadableItem;
}
