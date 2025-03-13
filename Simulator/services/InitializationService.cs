using CommonItems.Models;

namespace Simulator.Services;

class InitializationService
{
    private readonly ILogger<InitializationService> _logger;
    private readonly ProductionPlanManagement.Interfaces.ILoadProductionPlanService _loadProductionPlanService;
    private readonly CommonItems.Interfaces.ITransportRecordRegister _transportRecordRegister;
    private readonly WorkOrderManagement.Interfaces.IWorkOrderRegister _workOrderRegister;
    private readonly ShippingOperationCoordinator.Interfaces.IInitializationService _socInitializationService;
    private readonly InventoryPalletCoordinator.Interfaces.IInitializationService _ipcInitializationService;
    private readonly ShippingPalletCoordinator.Interfaces.IInitializationService _spcInitializationService;

    public InitializationService(
        ILogger<InitializationService> logger,
        ProductionPlanManagement.Interfaces.ILoadProductionPlanService loadProductionPlanService,
        CommonItems.Interfaces.ITransportRecordRegister transportRecordRegister,
        WorkOrderManagement.Interfaces.IWorkOrderRegister workOrderRegister,
        ShippingOperationCoordinator.Interfaces.IInitializationService socInitializationService,
        InventoryPalletCoordinator.Interfaces.IInitializationService ipcInitializationService,
        ShippingPalletCoordinator.Interfaces.IInitializationService spcInitializationService
    ) {
        _logger = logger;
        _loadProductionPlanService = loadProductionPlanService;
        _transportRecordRegister = transportRecordRegister;
        _workOrderRegister = workOrderRegister;
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
            _transportRecordRegister.Clear();
            resourceSetup();
            initialSetting();
        } catch (Exception ex) {
            _logger.LogError(ex, "シミュレータの初期化処理でエラーが発生しました");
            throw;
        }
    }
    void resourceSetup() {
        _socInitializationService.Initialize();
        _ipcInitializationService.Initialize();
        _spcInitializationService.Initialize();
    }
    void initialSetting() {
        loadProductionPlanAndOrder();
        setInitialShippingPallet();
    }
    void loadProductionPlanAndOrder() {
        var loadedPlans = _loadProductionPlanService.LoadProductionPlans();
        var paramPlans = loadedPlans.Select(x => new ProductionPlan(x.DeliveryDate, x.Line, x.Size, x.PalletNumber, x.Priority, x.Hinban)).ToList();
        _workOrderRegister.Clear();
        _workOrderRegister.Register(paramPlans);
    }
    void setInitialShippingPallet() {
        _spcInitializationService.SetInitialShippingPallet();
        _socInitializationService.TakeInitialShippingPallets();
        _socInitializationService.TakeInitialInventoryPallets();
    }
        
    record ProductionPlan(string DeliveryDate, string Line, string Size, int PalletNumber, int Priority, Hinban Hinban): WorkOrderManagement.Interfaces.IProductPlan;
}
