using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

/// <summary>
/// 出荷パレットのイベントを監視する Observer インターフェース
/// </summary>
public interface IShippingStorageEventObserver
{
    /// <summary>
    /// 出荷パレット返却イベント
    /// </summary>
    IObservable<IShippingPalletReturnEvent> ReturnEvent { get; }
}

/// <summary>
/// 出荷パレット返却イベント
/// </summary>
public interface IShippingPalletReturnEvent
{
    ShippingStationCode StationCode { get; }
}
