using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

class InventoryPalletManagementService: IInventoryPalletManagementService
{
    private readonly ILogger<InventoryPalletManagementService> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;

    public InventoryPalletManagementService(
        ILogger<InventoryPalletManagementService> logger,
        IInventoryPalletCoordinatorDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public void Clear() {
        _context.InventoryPallets.RemoveRange(_context.InventoryPallets);
        _context.SaveChanges();
    }
}
