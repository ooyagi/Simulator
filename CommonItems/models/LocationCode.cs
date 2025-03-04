namespace CommonItems.Models;

/// <summary>
/// ロケーションコード
/// </summary>
/// <param name="Value"></param>
public record LocationCode(string Value) {
    public static LocationCode Default => new LocationCode(DefaultLocationCode);

    private static string DefaultLocationCode = "DEFAULT_LOCATION";
}
