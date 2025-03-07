namespace CommonItems.Models;

public record ShippingPalletID
{
    public string DeliveryDate { get; set; } = "19000101000000";
    public string Category { get; set; } = "";
    public int PalletNumber { get; set; } = 0;
    public bool IsCustomPalette => DeliveryDate == CUSTOM_DELIVERY_DATE && Category == CUSTOM_CATEGORY && PalletNumber == CUSTOM_PALLET_NUMBER;
    public string Value => ToString();

    public static string CUSTOM_DELIVERY_DATE = "00000000000000";
    public static string CUSTOM_CATEGORY = "SEISANKEIKAKUGAI";
    public static int CUSTOM_PALLET_NUMBER = -1;

    public ShippingPalletID() { }
    public ShippingPalletID(string id) {
        var parts = id.Split('-');
        DeliveryDate = parts[0];
        Category = parts[1];
        PalletNumber = int.Parse(parts[2]);
    }
    public ShippingPalletID(
        string deliveryDate,
        string category,
        int palletNumber
    ) {
        DeliveryDate = deliveryDate;
        Category = category;
        PalletNumber = palletNumber;
    }
    public static ShippingPalletID CustomPaletteID => new ShippingPalletID(CUSTOM_DELIVERY_DATE, CUSTOM_CATEGORY, CUSTOM_PALLET_NUMBER);

    public override string ToString() => $"{DeliveryDate}-{Category}-{PalletNumber}";
}
