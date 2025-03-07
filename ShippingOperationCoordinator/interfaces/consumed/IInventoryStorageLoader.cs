using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IInventoryStorageLoader
{
    /// <summary>
    /// 指定された品番が在庫に存在するかどうかを返す
    /// </summary>
    bool IsExists(Hinban hinban);
    /// <summary>
    /// 指定された品番の在庫が指定された数以下であるかどうかを返す
    /// </summary>
    bool IsUseup(Hinban hinban, int loadableCount);
    /// <summary>
    /// 在庫情報を取得する
    /// </summary>
    IEnumerable<IInventoryPalletInfo> GetStoragedItems();
}
