using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface ITempStorageLoader
{
    /// <summary>
    /// ピックアップ可能なアイテムを取得する
    /// </summary>
    IEnumerable<IAvarableHinban> GetAvarableHinbans(ShippingStationCode stationCode);
    /// <summary>
    /// 指定された作業場所以外の作業場所においてピックアップ可能なアイテムを取得する
    /// </summary>
    IEnumerable<IAvarableHinban> GetAvarableHinbansInOtherStation(ShippingStationCode stationCode);
    /// <summary>
    /// 指定された品番の内、次回ピックアップで在庫数が 0になるアイテムを取得する
    /// </summary>
    IEnumerable<IAvarableHinban> GetItemsThatCanReduceInventoryToZero(IEnumerable<Hinban> hinbans);
    /// <summary>
    /// 指定された品番がピックアップ可能かどうかを判定する
    /// </summary>
    bool IsPickable(ShippingStationCode stationCode, Hinban hinban, int quantity = 1);
}

public interface IAvarableHinban
{
    LocationCode LocationCode { get; }
    Hinban Hinban { get; }
    int Quantity { get; }
}
