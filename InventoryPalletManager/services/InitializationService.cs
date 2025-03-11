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

    public InitializationService(
        ILogger<InitializationService> logger,
        IOptions<InventoryStorageConfig> options,
        ITemporaryStorageManagementService temporaryStorageManagementService
    ) {
        _logger = logger;
        _config = options.Value;
        _temporaryStorageManagementService = temporaryStorageManagementService;
    }

    public void Initialize() {
        _logger.LogInformation("在庫パレット制御初期化処理");
        try {
            _temporaryStorageManagementService.Clear();
            foreach(var tmp in _config.TemporaryStorages) {
                _logger.LogDebug($"一時置き場登録: 出荷作業場所 [{tmp.ShippingStationCode}] / コード [{tmp.LocationCode}]");
                var stationCode = new ShippingStationCode(tmp.ShippingStationCode);
                var locationCode = new LocationCode(tmp.LocationCode);
                _temporaryStorageManagementService.Add(stationCode, locationCode);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "在庫パレット制御初期化処理でエラーが発生しました");
            throw;
        }
    }
}
