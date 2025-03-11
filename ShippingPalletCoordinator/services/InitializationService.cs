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
    private readonly IShikakariStorageLoader _shikakariStorageLoader;
    private readonly IRotateShippingPalletService _rotateShippingPalletService;
    private readonly IShippingStorageManagementService _shippingStorageManagementService;
    private readonly IShikakariStorageManagementService _shikakariStorageManagementService;

    public InitializationService(
        ILogger<InitializationService> logger,
        IOptions<ShippingStorageConfig> config,
        IShikakariStorageLoader shikakariStorageLoader,
        IRotateShippingPalletService rotateShippingPalletService,
        IShippingStorageManagementService shippingStorageManagementService,
        IShikakariStorageManagementService shikakariStorageManagementService
    ) {
        _logger = logger;
        _config = config.Value;
        _shikakariStorageLoader = shikakariStorageLoader;
        _rotateShippingPalletService = rotateShippingPalletService;
        _shippingStorageManagementService = shippingStorageManagementService;
        _shikakariStorageManagementService = shikakariStorageManagementService;
    }

    /// <summary>
    /// 初期化処理
    /// 
    /// 出荷パレット置き場、仕掛パレット置き場の初期化を行う
    /// </summary>
    public void Initialize() {
        _logger.LogInformation("在庫パレット制御初期化処理");
        InitializeShippingStorage();
        InitializeShikakariStorage();
    }
    public void SetInitialShippingPallet() {
        _logger.LogInformation($"初期出荷パレット設定開始");
        try {
            var locations = _shikakariStorageLoader.GetEmptyLocations();
            foreach(var location in locations) {
                _logger.LogDebug($"初期出荷パレット設定: 仕掛パレット置き場 [{location.LocationCode}]");
                _rotateShippingPalletService.InboundNextPallet(location.LocationCode);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "初期出荷パレット設定でエラーが発生しました");
            throw;
        }
    }
    private void InitializeShippingStorage() {
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
    private void InitializeShikakariStorage() {
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
