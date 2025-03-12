using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class ShippingPalletLoader: Services.IShippingPalletLoader
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
}
