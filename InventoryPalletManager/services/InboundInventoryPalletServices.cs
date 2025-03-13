using Microsoft.Extensions.Logging;
using CommonItems.Models;
using InventoryPalletCoordinator.Models;
using InventoryPalletCoordinator.Interfaces;

namespace InventoryPalletCoordinator.Services;

class InboundInventoryPalletServices: IInboundInventoryPalletServices
{
    private readonly ILogger<InboundInventoryPalletServices> _logger;
    private readonly IInventoryPalletCoordinatorDbContext _context;
    private readonly IInventoryStorageManagementService _inventoryStorageManagementService;
    private readonly IInventoryStorageLoader _inventoryStorageLoader;

    public InboundInventoryPalletServices(
        ILogger<InboundInventoryPalletServices> logger,
        IInventoryPalletCoordinatorDbContext context,
        IInventoryStorageManagementService inventoryStorageManagementService,
        IInventoryStorageLoader inventoryStorageLoader
    ) {
        _logger = logger;
        _context = context;
        _inventoryStorageManagementService = inventoryStorageManagementService;
        _inventoryStorageLoader = inventoryStorageLoader;
    }

    /// <summary>
    /// 在庫パレット入庫サービス
    /// 
    /// 搬送数試算のためロケーションは無制限としている
    /// 空きロケーションが見つからない場合は新規ロケーションを作成する
    /// </summary>
    /// <remarks>
    /// 現在は在庫パレット置き場に直接新規パレットを置く形になっているが
    /// 本番システムでは搬入口に置かれたタイミングで在庫パレットを登録するので状況が異なる
    /// </remarks>
    public InventoryPallet Inbound(Hinban hinban) {
        var pallets = new InventoryPallet(hinban);
        var emptyStorage = _inventoryStorageLoader.FindEmptyLocation() ?? _inventoryStorageManagementService.AddAuto();
        emptyStorage.Place(pallets.Id);
        _context.InventoryPallets.Add(pallets);
        _context.SaveChanges();
        return pallets;
    }
}
