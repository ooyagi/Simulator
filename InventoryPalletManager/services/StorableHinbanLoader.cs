using Microsoft.Extensions.Options;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

class StorableHinbanLoader: IStorableHinbanLoader, Interfaces.IStorableHinbanLoader, ShippingOperationCoordinator.Interfaces.IStorableHinbanLoader
{
    private readonly IInventoryPalletCoordinatorDbContext _context;
    private readonly InventoryStorageConfig _config;

    public StorableHinbanLoader(
        IInventoryPalletCoordinatorDbContext context,
        IOptions<InventoryStorageConfig> config
    ) {
        _context = context;
        _config = config.Value;
    }

    public IEnumerable<Hinban> GetHinbans() {
        return _context.StorableHinbans.Where(x => x.Level <= _config.StorableHinbanLevel).Select(x => x.Hinban).ToList();
    }
    public bool IsStorable(Hinban hinban) {
        return _context.StorableHinbans.Any(x => x.Hinban == hinban && x.Level <= _config.StorableHinbanLevel);
    }
}
