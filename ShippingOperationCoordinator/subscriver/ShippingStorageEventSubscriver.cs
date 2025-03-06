using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

public class ShippingStorageEventSubscriber: IDisposable
{
    private readonly ILogger<ShippingStorageEventSubscriber> _logger;
    private IServiceProvider _serviceProvider;
    private List<IDisposable> _subscriptions = new List<IDisposable>();

    public ShippingStorageEventSubscriber(
        ILogger<ShippingStorageEventSubscriber> logger,
        IServiceProvider serviceProvider,
        IShippingStorageEventObserver observer
    ) {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _subscriptions.Add(observer.ReturnEvent.Subscribe(OnReturnEvent));
    }

    private void OnReturnEvent(IShippingPalletReturnEvent palletReturnEvent) {
        _logger.LogInformation($"出荷パレット返却イベントを受信しました： 出荷作業場所[{palletReturnEvent.StationCode}]");

        try {
            using var scope = _serviceProvider.CreateScope();
            var takeService = scope.ServiceProvider.GetRequiredService<TakeShippingPalletService>();
            takeService.Take(palletReturnEvent.StationCode);
        } catch (Exception ex) {
            _logger.LogError(ex, "出荷パレット取り寄せ処理中にエラーが発生しました");
        }
    }

    public void Dispose() {
        foreach (var subscription in _subscriptions) {
            subscription.Dispose();
        }
    }
}
