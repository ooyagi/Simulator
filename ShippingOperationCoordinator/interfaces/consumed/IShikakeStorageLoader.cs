using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IShikakariStorageLoader
{
    IEnumerable<IShikakariPalletLoadableHinbanInfo> GetInitialPallets();
    IEnumerable<IShikakariPalletLoadableHinbanInfo> GetLoadableFrom(IEnumerable<IInventoryPalletInfo> pallets);
    IEnumerable<ICompletablePalletInfo> FilterCompletableBy(IEnumerable<IInventoryPalletInfo> pallets);
}

public interface IShikakariPalletLoadableHinbanInfo
{
    LocationCode LocationCode { get; }
    ShippingPalletID ShippingPalletID { get; }
    Hinban? NextHinban { get; }
    Hinban? BlockHinban { get; }
    int RemainStep { get; }
    int RequiredHinbanTypeCount  { get; }   // 積み込める品番の種類数
    int BlockHinbanLoadableCount { get; }   // 積み込めるブロック品番の個数
    bool IsLoadable { get; }                // 作成時の条件で積み込み可能か

    bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity);
    bool IsCompletableBy(IInventoryPalletInfo pickableItem);

    IEnumerable<IEmptiablePalletInfo> GetEmptiablePallets(IEnumerable<IInventoryPalletInfo> inventoryPallets);
}
public interface IEmptiablePalletInfo
{
    LocationCode LocationCode { get; }
    Hinban Hinban { get; }
    int EmptiableStep { get; }
}
