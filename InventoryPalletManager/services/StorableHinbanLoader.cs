using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;

namespace InventoryPalletCoordinator.Services;

class StorableHinbanLoader: IStorableHinbanLoader, Interfaces.IStorableHinbanLoader, ShippingOperationCoordinator.Interfaces.IStorableHinbanLoader
{
    private readonly IInventoryPalletCoordinatorDbContext _context;

    public StorableHinbanLoader(
        IInventoryPalletCoordinatorDbContext context
    ) {
        _context = context;
    }

    public IEnumerable<Hinban> GetHinbans() {
        return _context.StorableHinbans.Select(x => x.Hinban).ToList();
    }
    public bool IsStorable(Hinban hinban) {
        return _context.StorableHinbans.Any(x => x.Hinban == hinban);
    }
}
