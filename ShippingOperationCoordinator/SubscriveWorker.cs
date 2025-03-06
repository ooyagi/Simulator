using Microsoft.Extensions.Hosting;
using ShippingOperationCoordinator.Services;

namespace ShippingOperationCoordinator;

class SubscriveWorker: IHostedService, IDisposable
{
    private readonly Task _completedTask = Task.CompletedTask;
    private ShippingStorageEventSubscriber _shippingStorageEventSubscriber;
    private TemporaryStorageEventSubscriber _temporaryStorageEventSubscriber;

    public SubscriveWorker(
        ShippingStorageEventSubscriber shippingStorageEventSubscriber,
        TemporaryStorageEventSubscriber temporaryStorageEventSubscriber
    ) {
        _shippingStorageEventSubscriber = shippingStorageEventSubscriber;
        _temporaryStorageEventSubscriber = temporaryStorageEventSubscriber;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        return _completedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
    public void Dispose() {
        _shippingStorageEventSubscriber.Dispose();
        _temporaryStorageEventSubscriber.Dispose();
    }
}
