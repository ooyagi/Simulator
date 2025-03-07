namespace CommonItems.Models;

public record InventoryPalletID(string Value)
{
    public static InventoryPalletID Default => new InventoryPalletID(DefaultPalletID);

    private static string DefaultPalletID = "DEFAULT_PALLET_ID";
}
