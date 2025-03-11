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
    private readonly IShippingStationManagementService _shippingStationManagementService;

    public InitializationService(
        ILogger<InitializationService> logger,
        IOptions<ShippingOperationSettings> options,
        IShippingStationManagementService shippingStationManagementService
    ) {
        _logger = logger;
        _config = options.Value;
        _shippingStationManagementService = shippingStationManagementService;
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
