using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

public class TemporaryStorageEventSubscriber: IDisposable
{
    private readonly ILogger<TemporaryStorageEventSubscriber> _logger;
    private IServiceProvider _serviceProvider;
    private List<IDisposable> _subscriptions = new List<IDisposable>();

    public TemporaryStorageEventSubscriber(
        ILogger<TemporaryStorageEventSubscriber> logger,
        IServiceProvider serviceProvider,
        ITemporaryStorageEventObserver observer
    ) {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _subscriptions.Add(observer.ReturnEvent.Subscribe(OnReturnEvent));
    }

    private void OnReturnEvent(IInventoryPalletReturnEvent palletReturnEvent) {
        _logger.LogInformation($"在庫パレット返却イベントを受信しました： 出荷作業場所[{palletReturnEvent.StationCode}]");

        try {
            using var scope = _serviceProvider.CreateScope();
            var takeService = scope.ServiceProvider.GetRequiredService<TakeInventoryPalletService>();
            takeService.Take(palletReturnEvent.StationCode);
        } catch (Exception ex) {
            _logger.LogError(ex, "在庫パレット取り寄せ処理中にエラーが発生しました");
        }
    }

    public void Dispose() {
        foreach (var subscription in _subscriptions) {
            subscription.Dispose();
        }
    }
}
