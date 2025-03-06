using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShippingOperationCoordinator.Interfaces;

namespace Simulator;
public class SimulatorHostedService: IHostedService, IDisposable
{
    private readonly ILogger<SimulatorHostedService> _logger;
    private readonly IShippingStationLoader _shippingStationLoader;
    private readonly ITransferService _transferService;
    private readonly IChangeInventoryPalletService _changeInventoryPalletService;
    private readonly IChangeShippingPalletService _changeShippingPalletService;
    private Timer? _timer;

    public SimulatorHostedService(
        ILogger<SimulatorHostedService> logger,
        IShippingStationLoader shippingStationLoader,
        ITransferService transferService,
        IChangeInventoryPalletService changeInventoryPalletService,
        IChangeShippingPalletService changeShippingPalletService
    ) {
        _logger = logger;
        _shippingStationLoader = shippingStationLoader;
        _transferService = transferService;
        _changeInventoryPalletService = changeInventoryPalletService;
        _changeShippingPalletService = changeShippingPalletService;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Simulator hosted service starting.");
        _timer = new Timer(ExecuteCycle, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        return Task.CompletedTask;
    }
    private void ExecuteCycle(object? state) {
        var stations = _shippingStationLoader.All();
        foreach (var station in stations) {
            bool transfer = _transferService.ExecuteTransfer(station.Code);
            bool changeShippingPallet = _changeShippingPalletService.Change(station.Code);
            bool changeEmptyPallet = _changeInventoryPalletService.ChangeEmptyPallet(station.Code);
            if (!transfer && !changeShippingPallet && !changeEmptyPallet) {
                _changeInventoryPalletService.Change(station.Code);
            }
        }
    }
    public Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Simulator hosted service stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() {
        _timer?.Dispose();
    }
}
