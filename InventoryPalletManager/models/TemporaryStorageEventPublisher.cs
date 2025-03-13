using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommonItems.Models;
using InventoryPalletCoordinator.Services;

namespace InventoryPalletCoordinator.Models;

/// <summary>
/// 一時置き場のイベントを管理するパブリッシャーおよびオブザーバー
/// </summary>
class TemporaryStorageEventPublisher: ITemporaryStorageEventPublisher, ShippingOperationCoordinator.Interfaces.ITemporaryStorageEventObserver, IDisposable
{
    private readonly ILogger<TemporaryStorageEventPublisher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Subject<ShippingOperationCoordinator.Interfaces.IInventoryPalletReturnEvent> _pickupEventSubject = new();

    public IObservable<ShippingOperationCoordinator.Interfaces.IInventoryPalletReturnEvent> ReturnEvent { get { return _pickupEventSubject.AsObservable(); } }


    public TemporaryStorageEventPublisher(
        ILogger<TemporaryStorageEventPublisher> logger,
        IServiceProvider serviceProvider
    ) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void PublishPickupEvent(LocationCode locationCode) {
        _logger.LogDebug($"[{locationCode.Value}] からのピックアップイベントを発行します");

        try {
            using var scope = _serviceProvider.CreateScope();
            var temporaryStorageLoader = scope.ServiceProvider.GetRequiredService<ITemporaryStorageLoader>();
            var stationCode = temporaryStorageLoader.ConvertStationCode(locationCode);
            if (stationCode == null) {
                _logger.LogError($"[{locationCode.Value}] は一時置き場のコードとして不正です");
                return;
            }
            var pickupEvent = new PickupEvent(stationCode);
            _pickupEventSubject.OnNext(pickupEvent);
        } catch (Exception ex) {
            _logger.LogError(ex, $"[{locationCode.Value}] からのピックアップイベントの発行に失敗しました");
        }
    }

    public void Dispose() {
        _pickupEventSubject.Dispose();
    }
}

/// <summary>
/// 一時置き場のイベントを表す
/// </summary>
public record PickupEvent(ShippingStationCode StationCode): ShippingOperationCoordinator.Interfaces.IInventoryPalletReturnEvent;
