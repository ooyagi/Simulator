namespace ShippingPalletCoordinator.Models;

public class ShippingStorageConfig
{
    public List<ShippingStorageSettings> ShippingStorages { get; set; } = new();
    public List<ShikakariStorageSettings> ShikakariStorages { get; set; } = new();
}

public class ShippingStorageSettings
{
    public string ShippingStationCode { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
}

public class ShikakariStorageSettings
{
    public string LocationCode { get; set; } = string.Empty;
}
