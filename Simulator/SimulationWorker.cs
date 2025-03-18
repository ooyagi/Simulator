using Simulator.Services;

namespace Simulator;

public class SimulationWorker: IHostedService, IDisposable
{
    private readonly ILogger<SimulationWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;
    private int _count = 0;

    public SimulationWorker(
        ILogger<SimulationWorker> logger,
        IServiceProvider serviceProvider
    ) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Simulator hosted service starting.");
        Thread.Sleep(1000);
        _timer = new Timer(ExecuteCycle, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(800));
        return Task.CompletedTask;
    }
    private void ExecuteCycle(object? state) {
        try {
            using var scope = _serviceProvider.CreateScope();
            var simulatorService = scope.ServiceProvider.GetRequiredService<ISimulationService>();
            simulatorService.Step(_count);
            _count++;
        } catch (Exception ex) {
            _logger.LogError(ex, "Simulator hosted service error.");
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
