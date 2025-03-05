using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface ITempStorageLoader
{
    /// <summary>
    /// 空パレットを取得する
    /// </summary>
    IEnumerable<IInventoryPalletInfo> GetEmptyPallets(ShippingStationCode stationCode);
    /// <summary>
    /// ピックアップ可能なアイテムを取得する
    /// </summary>
    IEnumerable<IInventoryPalletInfo> GetAvarableHinbans(ShippingStationCode stationCode);
    /// <summary>
    /// 指定された作業場所以外の作業場所においてピックアップ可能なアイテムを取得する
    /// </summary>
    IEnumerable<IInventoryPalletInfo> GetAvarableHinbansInOtherStation(ShippingStationCode stationCode);
    /// <summary>
    /// 指定された品番の内、次回ピックアップで在庫数が 0になるアイテムを取得する
    /// </summary>
    IEnumerable<IInventoryPalletInfo> GetItemsThatCanReduceInventoryToZero(IEnumerable<Hinban> hinbans);
    /// <summary>
    /// 指定された品番がピックアップ可能かどうかを判定する
    /// </summary>
    bool IsPickable(ShippingStationCode stationCode, Hinban hinban, int quantity = 1);
}
