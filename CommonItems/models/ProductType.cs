namespace CommonItems.Models;

/// <summary>
/// 製品の種別
/// 品番の先頭2文字から判別する
/// 現時点でサポートするのは床・浴槽の 2種
/// </summary>
public record ProductType(string Value);
