using CommonItems.Models;

namespace InventoryPalletCoordinator.Services;

/// <summary>
/// 一時置き場のイベントを通知するパブリッシャー
/// </summary>
public interface ITemporaryStorageEventPublisher
{
    /// <summary>
    /// 一時置き場からの引取通知を発行
    /// </summary>
    void PublishPickupEvent(LocationCode locationCode);
}
