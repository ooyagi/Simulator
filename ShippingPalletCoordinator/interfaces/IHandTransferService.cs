using CommonItems.Models;

namespace ShippingPalletCoordinator.Interfaces;

/// <summary>
/// 手動乗せ換えサービス
/// 
/// 本来は ShippingOperationCoordinator に属するが、今回は搬送回数の試算が目的であり
/// 手動の乗せ換えは仕掛パレット置き場の特定品番を乗せ換え完了にするだけなので
/// 暫定的に仕掛パレット側で行う
/// </summary>
public interface IHandTransferService
{
    bool ExecuteTransfer(IEnumerable<Hinban> storableHintans);
}
