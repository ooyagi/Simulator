using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IShippingStrageLoader
{
    IEnumerable<IShippingPalletInfo> All(ShippingStationCode stationCode);
    IEnumerable<ICompletablePalletInfo> FilterCompletableBy(ShippingStationCode stationCode, IEnumerable<ITransferablePalletInfo> pallets);
    IEnumerable<IShippingPalletLoadableHinbanInfo> GetLoadableFrom(ShippingStationCode stationCode, IEnumerable<ITransferablePalletInfo> pallets);
}

// Returns
public interface IShippingPalletInfo
{
    LocationCode LocationCode { get; }
    ShippingPalletID ShippingPalletID { get; }
    Hinban NextHinban { get; }
    bool IsCompleted { get; }
}
public interface ICompletablePalletInfo
{
    LocationCode LocationCode { get; }
    Hinban NextHinban { get; }
    int Step { get; }
}
public interface IShippingPalletLoadableHinbanInfo
{
    LocationCode LocationCode { get; }
    ShippingPalletID ShippingPalletID { get; }
    Hinban NextHinban { get; }
    int Step { get; }               // 出荷パレット完了までに積込が必要なステップ数（在庫の有無によらない）
    int LoadableItemCount { get; }  // 入力された情報で積み込み可能なアイテム数

    // 対象品番の積み込み可能数が指定された数量以上かどうか
    bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity);
}
public interface ILoadableHinbanInfo
{
    Hinban Hinban { get; }
    int Quantity { get; }
}

// Params
public interface ITransferablePalletInfo
{
    LocationCode LocationCode { get; }
    Hinban Hinban { get; }
    int Quantity { get; }
}
