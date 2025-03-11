namespace InventoryPalletCoordinator.Models;

public class InventoryStorageConfig
{
    public List<TemporaryStorageSetting> TemporaryStorages { get; set; } = new();
}

public class TemporaryStorageSetting
{
    public string ShippingStationCode { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
}
