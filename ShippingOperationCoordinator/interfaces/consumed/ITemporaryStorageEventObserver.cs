using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

/// <summary>
/// 出荷パレットのイベントを監視する Observer インターフェース
/// </summary>
public interface ITemporaryStorageEventObserver
{
    /// <summary>
    /// 出荷パレット返却イベント
    /// </summary>
    IObservable<IInventoryPalletReturnEvent> ReturnEvent { get; }
}

/// <summary>
/// 出荷パレット返却イベント
/// </summary>
public interface IInventoryPalletReturnEvent
{
    ShippingStationCode StationCode { get; }
}
