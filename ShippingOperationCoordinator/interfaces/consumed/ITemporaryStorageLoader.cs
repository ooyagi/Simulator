using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface ITempStorageLoader
{
    /// <summary>
    /// 空の一時置き場を取得する
    /// </summary>
    IEnumerable<LocationCode> GetEmptyLocationCodes(ShippingStationCode stationCode);
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
    /// 指定された品番がピックアップ可能かどうかを判定する
    /// </summary>
    bool IsPickable(ShippingStationCode stationCode, Hinban hinban, int quantity = 1);
    /// <summary>
    /// 指定された品番が取り寄せ可能かどうかを判定する
    /// 
    /// 他の出荷作業場所に搬送されている場合は取り寄せできない
    /// </summary>
    bool IsTakable(ShippingStationCode stationCode, Hinban hinban);
    /// <summary>
    /// 指定された品番が他の出荷作業場所に搬送されているかどうかを判定する
    /// </summary>
    bool InOtherStation(ShippingStationCode stationCode, Hinban hinban);
}
