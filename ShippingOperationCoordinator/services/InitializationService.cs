using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;
using ShippingOperationCoordinator.Models;

namespace ShippingOperationCoordinator.Services;

class InitializationService: IInitializationService
{
    private readonly ILogger<InitializationService> _logger;
    private readonly ShippingOperationSettings _config;
    private readonly IShippingStationLoader _shippingStationLoader;
    private readonly IShippingStationManagementService _shippingStationManagementService;
    private readonly TakeShippingPalletService _takeShippingPalletService;
    private readonly TakeInventoryPalletService _takeInventoryPalletService;

    public InitializationService(
        ILogger<InitializationService> logger,
        IOptions<ShippingOperationSettings> options,
        IShippingStationLoader shippingStationLoader,
        IShippingStationManagementService shippingStationManagementService,
        TakeShippingPalletService takeShippingPalletService,
        TakeInventoryPalletService takeInventoryPalletService
    ) {
        _logger = logger;
        _config = options.Value;
        _shippingStationLoader = shippingStationLoader;
        _shippingStationManagementService = shippingStationManagementService;
        _takeShippingPalletService = takeShippingPalletService;
        _takeInventoryPalletService = takeInventoryPalletService;
    }

    public void TakeInitialShippingPallets() {
        _logger.LogInformation("初期出荷パレット取り寄せ処理");
        try {
            var stations = _shippingStationLoader.All();
            foreach(var station in stations) {
                _logger.LogDebug($"初期出荷パレット取り寄せ: 出荷作業場所 [{station.Code}]");
                _takeShippingPalletService.TakeInitialPallets(station.Code);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "初期出荷パレット取り寄せ処理でエラーが発生しました");
            throw;
        }
    }
    public void TakeInitialInventoryPallets() {
        _logger.LogInformation("初期在庫パレット取り寄せ処理");
        try {
            var stations = _shippingStationLoader.All();
            foreach(var station in stations) {
                _logger.LogDebug($"初期在庫パレット取り寄せ: 出荷作業場所 [{station.Code}]");
                _takeInventoryPalletService.Take(station.Code);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "初期在庫パレット取り寄せ処理でエラーが発生しました");
            throw;
        }
    }
    public void Initialize() {
        _logger.LogInformation("出荷作業管理初期化処理");
        try {
            _shippingStationManagementService.Clear();
            foreach(var tmp in _config.ShippingStations) {
                _logger.LogDebug($"出荷作業場所登録: コード [{tmp.Code}]");
                var stationCode = new ShippingStationCode(tmp.Code);
                _shippingStationManagementService.Add(stationCode);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "出荷作業管理初期化処理でエラーが発生しました");
            throw;
        }
    }
}
