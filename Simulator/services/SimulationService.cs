using ShippingOperationCoordinator.Interfaces;

namespace Simulator.Services;

public class SimulationService: ISimulationService
{
    private readonly ILogger<SimulationService> _logger;
    private readonly IShippingStationLoader _shippingStationLoader;
    private readonly ITransferService _transferService;
    private readonly InventoryPalletCoordinator.Interfaces.IStorableHinbanLoader _storableHinbanLoader; // 本番では使用しない
    private readonly ShippingPalletCoordinator.Interfaces.IHandTransferService _handTransferService;    // 本番では出荷パレット管理では行わない
    private readonly IChangeInventoryPalletService _changeInventoryPalletService;
    private readonly IChangeShippingPalletService _changeShippingPalletService;

    public SimulationService(
        ILogger<SimulationService> logger,
        IShippingStationLoader shippingStationLoader,
        ITransferService transferService,
        InventoryPalletCoordinator.Interfaces.IStorableHinbanLoader storableHinbanLoader,
        ShippingPalletCoordinator.Interfaces.IHandTransferService handTransferService,
        IChangeInventoryPalletService changeInventoryPalletService,
        IChangeShippingPalletService changeShippingPalletService
    ) {
        _logger = logger;
        _shippingStationLoader = shippingStationLoader;
        _transferService = transferService;
        _storableHinbanLoader = storableHinbanLoader;
        _handTransferService = handTransferService;
        _changeInventoryPalletService = changeInventoryPalletService;
        _changeShippingPalletService = changeShippingPalletService;
    }

    public void Step() {
        _logger.LogInformation("シミュレーションステップを実行します");

        // 手動で積み替える品番は即時積み込まれるものとする
        var storableHintans = _storableHinbanLoader.GetHinbans().ToList();
        _ = _handTransferService.ExecuteTransfer(storableHintans);

        var stations = _shippingStationLoader.All();
        foreach (var station in stations) {
            _logger.LogInformation($"出荷作業場所 [{station.Code}] の処理を実行します");

            bool transfer = _transferService.ExecuteTransfer(station.Code);
            if (transfer) {
                continue;
            }
            bool changeShippingPallet = _changeShippingPalletService.Change(station.Code);
            bool changeEmptyPallet = _changeInventoryPalletService.ChangeEmptyPallet(station.Code);
            if (changeShippingPallet || changeEmptyPallet) {
                continue;
            }
            _ = _changeInventoryPalletService.Change(station.Code);
        }

    }
}
