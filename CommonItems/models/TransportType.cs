namespace CommonItems.Models;

public record TransportType(string Value)
{
    public static TransportType Unknown => new TransportType("Unknown");
    public static TransportType ReturnInventoryPallet => new TransportType("ReturnInventoryPallet");
    public static TransportType TakeInventoryPallet => new TransportType("TakeInventoryPallet");
    public static TransportType ReturnShippingPallet => new TransportType("ReturnShippingPallet");
    public static TransportType TakeShippingPallet => new TransportType("TakeShippingPallet");

    public static string _unnown = "Unknown";
    public static string _returnInventoryPallet = "ReturnInventoryPallet";
    public static string _takeInventoryPallet = "TakeInventoryPallet";
    public static string _returnShippingPallet = "ReturnShippingPallet";
    public static string _takeShippingPallet = "TakeShippingPallet";
}
