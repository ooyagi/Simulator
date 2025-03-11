namespace ShippingOperationCoordinator.Models;

public class ShippingOperationSettings
{
    public List<ShippingStationSetting> ShippingStations { get; set; } = new();
}

public class ShippingStationSetting
{
    public string Code { get; set; } = string.Empty;
}
