using ShippingPalletCoordinator.Interfaces;

namespace ShippingPalletCoordinator.Services;

class ShippingPalletManagementService: IShippingPalletManagementService
{
    private readonly IShippingPalletCoordinatorDbContext _context;

    public ShippingPalletManagementService(
        IShippingPalletCoordinatorDbContext context
    ) {
        _context = context;
    }

    public void Clear() {
        var shippingPallets = _context.ShippingPallets;
        _context.ShippingPallets.RemoveRange(shippingPallets);
        _context.SaveChanges();
    }
}
