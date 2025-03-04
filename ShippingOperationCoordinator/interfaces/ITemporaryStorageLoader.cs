using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface ITempStorageLoader
{
    /// <summary>
    /// ピックアップ可能なアイテムを取得する
    /// </summary>
    IEnumerable<IAvarableHinban> GetAvarableHinbans(ShippingStationCode stationCode);
    /// <summary>
    /// 指定された品番の内、次回ピックアップで在庫数が 0になるアイテムを取得する
    /// </summary>
    IEnumerable<IAvarableHinban> GetItemsThatCanReduceInventoryToZero(IEnumerable<Hinban> hinbans);
}

public interface IAvarableHinban
{
    LocationCode LocationCode { get; }
    Hinban Hinban { get; }
    int Quantity { get; }
}
