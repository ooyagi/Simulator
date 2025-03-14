using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IShippingStorageLoader
{
    IEnumerable<LocationCode> GetEmptyLocationCodes(ShippingStationCode stationCode);
    IEnumerable<IShippingPalletInfo> All(ShippingStationCode stationCode);
    IEnumerable<ICompletablePalletInfo> FilterCompletableBy(ShippingStationCode stationCode, IEnumerable<IInventoryPalletInfo> pallets);
    IEnumerable<IShippingPalletLoadableHinbanInfo> GetLoadableFrom(ShippingStationCode stationCode, IEnumerable<IInventoryPalletInfo> pallets);
}

// Returns
public interface IShippingPalletInfo
{
    LocationCode LocationCode { get; }
    ShippingPalletID? ShippingPalletID { get; }
    Hinban? NextHinban { get; }
    bool IsCompleted { get; }
}
public interface ICompletablePalletInfo
{
    LocationCode LocationCode { get; }
    ShippingPalletID ShippingPalletID { get; }
    Hinban NextHinban { get; }
    int RemainStep { get; }
}
public interface IShippingPalletLoadableHinbanInfo
{
    LocationCode LocationCode { get; }
    ShippingPalletID ShippingPalletID { get; }
    Hinban? NextHinban { get; }
    Hinban? BlockHinban { get; }
    int RemainStep { get; }
    int RequiredHinbanTypeCount  { get; }  // 積み込める品番の種類数

    // 対象品番の積み込み可能数が指定された数量以上かどうか
    bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity);
    bool IsCompletableBy (IInventoryPalletInfo pickableItem);
}
public interface ILoadableHinbanInfo
{
    Hinban Hinban { get; }
    int Quantity { get; }
}

// Params
public interface IInventoryPalletInfo
{
    LocationCode LocationCode { get; }
    Hinban Hinban { get; }
    int Quantity { get; }
}
