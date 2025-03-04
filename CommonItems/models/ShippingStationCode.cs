namespace CommonItems.Models;

public record ShippingStationCode
{
    public string Value { get; set; } = "";

    public ShippingStationCode() { }
    public ShippingStationCode(
        string value
    ) {
        Value = value;
    }
}
