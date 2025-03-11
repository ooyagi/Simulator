using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CommonItems.Models;
using ShippingPalletCoordinator.Models;
using ShippingPalletCoordinator.Interfaces;

namespace ShippingPalletCoordinator.Services;

class InitializationService: IInitializationService
{
    private readonly ILogger<InitializationService> _logger;
    private readonly ShippingStorageConfig _config;
    private readonly IShippingStorageManagementService _shippingStorageManagementService;
    private readonly IShikakariStorageManagementService _shikakariStorageManagementService;

    public InitializationService(
        ILogger<InitializationService> logger,
        IOptions<ShippingStorageConfig> config,
        IShippingStorageManagementService shippingStorageManagementService,
        IShikakariStorageManagementService shikakariStorageManagementService
    ) {
        _logger = logger;
        _config = config.Value;
        _shippingStorageManagementService = shippingStorageManagementService;
        _shikakariStorageManagementService = shikakariStorageManagementService;
    }

    public void Initialize() {
        _logger.LogInformation("在庫パレット制御初期化処理");
        InitializeShippingStorage();
        InitializeShikakariStorage();
    }
    public void InitializeShippingStorage() {
        _logger.LogInformation("出荷パレット置き場初期化");
        try {
            _shippingStorageManagementService.Clear();
            foreach(var tmp in _config.ShippingStorages) {
                _logger.LogDebug($"出荷パレット置き場登録: 出荷作業場所 [{tmp.ShippingStationCode}] / コード [{tmp.LocationCode}]");
                var stationCode = new ShippingStationCode(tmp.ShippingStationCode);
                var locationCode = new LocationCode(tmp.LocationCode);
                _shippingStorageManagementService.Add(stationCode, locationCode);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "出荷パレット制御初期化処理でエラーが発生しました");
            throw;
        }
    }
    public void InitializeShikakariStorage() {
        _logger.LogInformation("仕掛パレット置き場初期化");
        try {
            _shikakariStorageManagementService.Clear();
            foreach(var tmp in _config.ShikakariStorages) {
                _logger.LogDebug($"仕掛パレット置き場登録: コード [{tmp.LocationCode}]");
                var locationCode = new LocationCode(tmp.LocationCode);
                _shikakariStorageManagementService.Add(locationCode);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "仕掛パレット制御初期化処理でエラーが発生しました");
            throw;
        }
    }
}
