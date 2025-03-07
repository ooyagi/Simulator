using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

class InventoryStorageManagementService: IInventoryStorageManagementService
{
    private readonly ILogger<InventoryStorageManagementService> _logger;
    private readonly IInventoryStorageLoader _inventoryStorageLoader;
    private readonly IInventoryPalletCoordinatorDbContext _context;

    public InventoryStorageManagementService(
        ILogger<InventoryStorageManagementService> logger,
        IInventoryStorageLoader inventoryStorageLoader,
        IInventoryPalletCoordinatorDbContext context
    ) {
        _logger = logger;
        _inventoryStorageLoader = inventoryStorageLoader;
        _context = context;
    }

    /// <summary>
    /// 在庫パレット置き場の自動拡張
    /// </summary>
    public InventoryStorage AddAuto() {
        var nextIndex = _inventoryStorageLoader.GetLastIndex() + 1;
        var locationCode = new LocationCode($"LOC_{nextIndex.ToString("D3")}");
        var newStorage = new InventoryStorage(locationCode, nextIndex);
        _context.InventoryStorages.Add(newStorage);
        _context.SaveChanges();
        return newStorage;
    }
}
