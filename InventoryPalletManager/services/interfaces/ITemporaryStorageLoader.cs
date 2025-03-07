using CommonItems.Models;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

/// <summary>
/// 一時置き場の情報を取得するローダー
/// </summary>
interface ITemporaryStorageLoader
{
    ShippingStationCode? ConvertStationCode(LocationCode locationCode);
    TemporaryStorage? Find(LocationCode locationCode);
}

interface ITemporaryStorageInfo
{
    LocationCode LocationCode { get; }
    StorageStatus Status { get; }
    InventoryPalletID? InventoryPalletID { get; }
}

