namespace CommonItems.Models;

public record TransportType(string Value)
{
    public static TransportType Unknown => new TransportType(_unnown);
    public static TransportType ReturnInventoryPallet => new TransportType(_returnInventoryPallet);
    public static TransportType TakeInventoryPallet => new TransportType(_takeInventoryPallet);
    public static TransportType ReturnShippingPallet => new TransportType(_returnShippingPallet);
    public static TransportType TakeShippingPallet => new TransportType(_takeShippingPallet);

    private static string _unnown = "Unknown";
    private static string _returnInventoryPallet = "ReturnInventoryPallet";
    private static string _takeInventoryPallet = "TakeInventoryPallet";
    private static string _returnShippingPallet = "ReturnShippingPallet";
    private static string _takeShippingPallet = "TakeShippingPallet";
}
