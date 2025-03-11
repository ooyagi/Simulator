namespace Simulator.Services;

class InitializationService
{
    private readonly ILogger<InitializationService> _logger;
    private readonly ShippingOperationCoordinator.Interfaces.IInitializationService _socInitializationService;
    private readonly InventoryPalletCoordinator.Interfaces.IInitializationService _ipcInitializationService;
    private readonly ShippingPalletCoordinator.Interfaces.IInitializationService _spcInitializationService;

    public InitializationService(
        ILogger<InitializationService> logger,
        ShippingOperationCoordinator.Interfaces.IInitializationService socInitializationService,
        InventoryPalletCoordinator.Interfaces.IInitializationService ipcInitializationService,
        ShippingPalletCoordinator.Interfaces.IInitializationService spcInitializationService
    ) {
        _logger = logger;
        _socInitializationService = socInitializationService;
        _ipcInitializationService = ipcInitializationService;
        _spcInitializationService = spcInitializationService;
    }

    /// <summary>
    /// シミュレータの初期化処理
    /// </summary>
    /// <remarks>
    /// 出荷パレット制御、在庫パレット制御の初期化処理で出荷作業場所を参照するため
    /// 必ず最初に出荷作業制御の初期化処理を行う
    /// </remarks>
    public void Initialize() {
        _logger.LogInformation("シミュレータの初期化処理");

        try {
            _socInitializationService.Initialize();
            _ipcInitializationService.Initialize();
            _spcInitializationService.Initialize();
        } catch (Exception ex) {
            _logger.LogError(ex, "シミュレータの初期化処理でエラーが発生しました");
            throw;
        }
    }
}
