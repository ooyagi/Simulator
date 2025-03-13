using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

class InitializationService: IInitializationService
{
    private readonly ILogger<InitializationService> _logger;
    private readonly InventoryStorageConfig _config;
    private readonly ITemporaryStorageManagementService _temporaryStorageManagementService;
    private readonly IInventoryStorageManagementService _inventoryStorageManagementService;
    private readonly IInventoryPalletManagementService _inventoryPalletManagementService;

    public InitializationService(
        ILogger<InitializationService> logger,
        IOptions<InventoryStorageConfig> options,
        ITemporaryStorageManagementService temporaryStorageManagementService,
        IInventoryStorageManagementService inventoryStorageManagementService,
        IInventoryPalletManagementService inventoryPalletManagementService
    ) {
        _logger = logger;
        _config = options.Value;
        _temporaryStorageManagementService = temporaryStorageManagementService;
        _inventoryStorageManagementService = inventoryStorageManagementService;
        _inventoryPalletManagementService = inventoryPalletManagementService;
    }

    /// <summary>
    /// 在庫パレット制御初期化処理
    /// 
    /// 一時置き場のクリアと設定からの再登録を行う。
    /// 合わせて在庫パレット置き場と在庫パレットのクリアを行う。
    /// 
    /// 在庫パレットと在庫パレット置き場は必要に応じて作成するため初期化時には作成しない。
    /// </summary>
    public void Initialize() {
        _logger.LogInformation("在庫パレット制御初期化処理");
        try {
            InitializeTemporaryStorage();
            InitializeInventoryStorage();
        } catch (Exception ex) {
            _logger.LogError(ex, "在庫パレット制御初期化処理でエラーが発生しました");
            throw;
        }
    }
    private void InitializeTemporaryStorage() {
        _temporaryStorageManagementService.Clear();
        foreach(var tmp in _config.TemporaryStorages) {
            _logger.LogDebug($"一時置き場登録: 出荷作業場所 [{tmp.ShippingStationCode}] / コード [{tmp.LocationCode}]");
            var stationCode = new ShippingStationCode(tmp.ShippingStationCode);
            var locationCode = new LocationCode(tmp.LocationCode);
            _temporaryStorageManagementService.Add(stationCode, locationCode);
        }
    }
    private void InitializeInventoryStorage() {
        _inventoryStorageManagementService.Clear();
        _inventoryPalletManagementService.Clear();
    }
}
