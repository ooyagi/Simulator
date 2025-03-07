using CommonItems.Models;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

/// <summary>
/// 一時置き場の情報を取得するローダー
/// </summary>
interface ITemporaryStorageLoader
{
    ShippingStationCode? ConvertStationCode(LocationCode locationCode);
    ITemporaryStorageInfo? Find(LocationCode locationCode);
}

interface ITemporaryStorageInfo
{
    LocationCode LocationCode { get; }
    TemporaryStorageStatus Status { get; }
    InventoryPalletID InventoryPalletID { get; }
}

enum TemporaryStorageStatus: int {
    Empty = 0,
    Orderd = 1,
    InUse = 2,
    Return = 3
}
