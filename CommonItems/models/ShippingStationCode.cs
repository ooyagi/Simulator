namespace CommonItems.Models;

public record ShippingStationCode
{
    public string Value { get; set; } = "";

    public static ShippingStationCode Default => new ShippingStationCode(_defaultCode);
    public static string _defaultCode  = "DefaultStation";

    public ShippingStationCode() { }
    public ShippingStationCode(
        string value
    ) {
        Value = value;
    }
}
