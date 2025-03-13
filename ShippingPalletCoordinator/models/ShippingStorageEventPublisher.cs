using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommonItems.Models;
using ShippingPalletCoordinator.Services;
using Microsoft.EntityFrameworkCore.Internal;

namespace ShippingPalletCoordinator.Models;

/// <summary>
/// 一時置き場のイベントを管理するパブリッシャーおよびオブザーバー
/// </summary>
class ShippingStorageEventPublisher: IShippingStorageEventPublisher, ShippingOperationCoordinator.Interfaces.IShippingStorageEventObserver, IDisposable
{
    private readonly ILogger<ShippingStorageEventPublisher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Subject<ShippingOperationCoordinator.Interfaces.IShippingPalletReturnEvent> _pickupEventSubject = new();

    public IObservable<ShippingOperationCoordinator.Interfaces.IShippingPalletReturnEvent> ReturnEvent { get { return _pickupEventSubject.AsObservable(); } }


    public ShippingStorageEventPublisher(
        ILogger<ShippingStorageEventPublisher> logger,
        IServiceProvider serviceProvider
    ) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void PublishPickupEvent(LocationCode locationCode) {
        _logger.LogDebug($"[{locationCode.Value}] からのピックアップイベントを発行します");

        try {
            using var scope = _serviceProvider.CreateScope();
            var shippingStorageLoader = scope.ServiceProvider.GetRequiredService<IShippingStorageLoader>();
            var stationCode = shippingStorageLoader.ConvertStationCode(locationCode);
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
public record PickupEvent(ShippingStationCode StationCode): ShippingOperationCoordinator.Interfaces.IShippingPalletReturnEvent;
