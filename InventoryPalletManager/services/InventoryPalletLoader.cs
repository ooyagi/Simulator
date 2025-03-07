using Microsoft.Extensions.Logging;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;
using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

class InventoryPalletLoader: IInventoryPalletLoader
{
    private readonly ILogger<InventoryPalletLoader> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;

    public InventoryPalletLoader(
        ILogger<InventoryPalletLoader> logger,
        IInventoryPalletCoordinatorDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public InventoryPallet? Find(InventoryPalletID palletId) {
        return _context.InventoryPallets.FirstOrDefault(x => x.Id == palletId);
    }
    public IEnumerable<InventoryPallet> FliterByHinban(Hinban hinban) {
        return _context.InventoryPallets.Where(x => x.Hinban == hinban).OrderBy(x => x.Quantity).ToList();
    }
}
