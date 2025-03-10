namespace CommonItems.Models;

public record TransportType(string Value)
{
    public static TransportType Unknown => new TransportType("Unknown");
    public static TransportType ReturnInventoryPallet => new TransportType("ReturnInventoryPallet");
    public static TransportType TakeInventoryPallet => new TransportType("TakeInventoryPallet");
    public static TransportType ReturnShippingPallet => new TransportType("ReturnShippingPallet");
    public static TransportType TakeShippingPallet => new TransportType("TakeShippingPallet");

    private static string _unnown = "Unknown";
    private static string _returnInventoryPallet = "ReturnInventoryPallet";
    private static string _takeInventoryPallet = "TakeInventoryPallet";
    private static string _returnShippingPallet = "ReturnShippingPallet";
    private static string _takeShippingPallet = "TakeShippingPallet";
}
