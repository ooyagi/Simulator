using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IShippingPalletLoader
{
    IEnumerable<IShippingPalletLoadableHinbanInfoNoLocation> GetLoadableFrom(IEnumerable<IInventoryPalletInfo> pallets);
}

public interface IShippingPalletLoadableHinbanInfoNoLocation
{
    ShippingPalletID ShippingPalletID { get; }
    Hinban? NextHinban { get; }
    Hinban? BlockHinban { get; }
    int RemainStep { get; }
    int RequiredHinbanTypeCount  { get; }  // 積み込める品番の種類数

    // 対象品番の積み込み可能数が指定された数量以上かどうか
    bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity);
    bool IsCompletableBy (IInventoryPalletInfo pickableItem);
}
