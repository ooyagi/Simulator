using System.ComponentModel;
using CommonItems.Models;
using ShippingPalletCoordinator.Models;

public class CheckBlockItemTests
{
    private static readonly Hinban testHinban1 = new Hinban("FA-1609P(8)BV/U61");
    private static readonly Hinban testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
    private static readonly Hinban testHinban3 = new Hinban("FA-1209P(5)NV/FG105");

    [Theory]
    [MemberData(nameof(GetBlockHinbanTestData))]
    internal void GetBlockHinban_ReturnsExpectedResult(
        IEnumerable<ShippingPalletItem> palletItems,
        IEnumerable<ILoadableItem> loadableItems,
        Hinban? expectedBlockHinban,
        int expectedBlockCount
    ) {
        var (blockHinban, blockHinbanLoadableCount) = CheckBlockItem.GetBlockHinban(palletItems, loadableItems);

        Assert.Equal(expectedBlockHinban, blockHinban);
        Assert.Equal(expectedBlockCount, blockHinbanLoadableCount);
    }

    public static IEnumerable<object?[]> GetBlockHinbanTestData() {
        yield return new object?[] {
            // すべての品番が積み込み可能 → ブロック品番なし
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban2, 1)
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 1),
                new LocalLoadableItem(testHinban2, 1)
            },
            null, // ブロック品番なし
            0     // 積み込み可能数も 0
        };
        yield return new object?[] {
            // 途中で積み込みができない品番が発生する
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban2, 1),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban3, 2)
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 1),
                new LocalLoadableItem(testHinban2, 1)
                // testHinban3 の在庫なし → ブロック品番になる
            },
            testHinban3, // ブロック品番は testHinban3
            1            // testHinban3 の積み込み可能数
        };
        yield return new object?[] {
            // ブロック品番が連続する場合
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban3, 0),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban3, 1),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 2)
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 1)
                // testHinban3 の在庫なし → ブロック品番になる
            },
            testHinban3, // ブロック品番は testHinban3
            2            // testHinban3 の積み込み可能数 (連続して2個ある)
        };
        yield return new object?[] {
            // すでにすべての品番が積み込み済み → ブロック品番なし
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0) { IsCompleted = true },
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban2, 1) { IsCompleted = true }
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 1),
                new LocalLoadableItem(testHinban2, 1)
            },
            null, // すでに完了しているのでブロック品番なし
            0     // 積み込み可能数も 0
        };
    }

    private record LocalLoadableItem(Hinban Hinban, int Quantity) : ILoadableItem;
}
