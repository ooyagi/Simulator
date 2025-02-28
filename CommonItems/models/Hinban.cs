using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace CommonItems.Models;

/// <summary>
/// 品番
/// 製品の種類を特定する番号
/// </summary>
public record Hinban 
{
    public string Value { get; init; }

    [NotMapped]
    public ProductType Type { get; init; }
    [NotMapped]
    public string TypeNumber { get; init; }
    [NotMapped]
    public string Color { get; init; }

    public static Hinban Default => new(DEFAULT_VALUE);

    public static string DEFAULT_VALUE = $"XX-DMYTYPE/DMYCOLOR";
    private static string regExPattern = @"^([^/-]+)-(.+)/(.+)$";

    public Hinban(string value) {
        Value = value;
        Match match = Regex.Match(Value, regExPattern);
        if (!match.Success) {
            throw new ArgumentException("Invalid Hinban format: " + Value);
        }
        Type = new ProductType(match.Groups[1].Value);
        TypeNumber = match.Groups[2].Value;
        Color = match.Groups[3].Value;
    }
}
