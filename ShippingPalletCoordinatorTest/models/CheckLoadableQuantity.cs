using CommonItems.Models;
using ShippingPalletCoordinator.Models;

public class CheckLoadableQuantityTests
{
    private static readonly Hinban testHinban1 = new Hinban("FA-1609P(8)BV/U61");
    private static readonly Hinban testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
    private static readonly Hinban testHinban3 = new Hinban("FA-1209P(5)NV/FG105");

    [Theory]
    [MemberData(nameof(GetLoadableQuantityTestData))]
    internal void GetLoadableQuantity_ReturnsExpectedCount(
        IEnumerable<ShippingPalletItem> palletItems,
        IEnumerable<ILoadableItem> loadableItems,
        Hinban hinban,
        int expectedCount
    ) {
        var result = CheckLoadableQuantity.GetLoadableQuantity(palletItems, loadableItems, hinban);

        Assert.Equal(expectedCount, result);
    }

    public static IEnumerable<object[]> GetLoadableQuantityTestData() {
        yield return new object[] {
            // 積み込み可能な製品なし
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban2, 1),
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban3, 2),
            },
            testHinban1, // 検証する品番
            0 // 1回積み込み可能
        };
        yield return new object[] {
            // すべての品番が積み込み可能
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban2, 1),
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 2),
                new LocalLoadableItem(testHinban2, 2)
            },
            testHinban1, // 検証する品番
            1 // 1回積み込み可能
        };
        yield return new object[] {
            // 途中で積み込み不能な品番が出現
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban3, 1),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 2),
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 2)
                // testHinban3 が不足 → 途中で停止
            },
            testHinban1,
            1 // testHinban1 のみ積み込める
        };
        yield return new object[] {
            // すでにすべての品番が積み込み済み
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0) { IsCompleted = true },
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban2, 1) { IsCompleted = true },
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 2),
                new LocalLoadableItem(testHinban2, 2)
            },
            testHinban1,
            0 // すでに完了しているため積み込み不要
        };
    }

    [Theory]
    [MemberData(nameof(GetEmptiablePalletsTestData))]
    internal void GetEmptiablePallets_ReturnsCorrectEmptiablePallets(
        IEnumerable<ShippingPalletItem> palletItems,
        IEnumerable<ILoadableItem> loadableItems,
        IEnumerable<(Hinban, int)> expectedEmptiablePallets
    ) {
        var result = CheckLoadableQuantity.GetEmptiablePallets(palletItems, loadableItems);

        Assert.Equal(expectedEmptiablePallets, result);
    }

    public static IEnumerable<object[]> GetEmptiablePalletsTestData() {
        yield return new object[] {
            // 使い切れる品番がない場合
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban2, 1),
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban3, 1)
            },
            new List<(Hinban, int)> { }
        };
        yield return new object[] {
            // 使い切れる品番がある場合
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban2, 1),
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 1),
                new LocalLoadableItem(testHinban2, 1)
            },
            new List<(Hinban, int)> {
                (testHinban1, 1),
                (testHinban2, 2)
            }
        };
        yield return new object[] {
            // 途中で積み込み不能になる場合
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban3, 1),
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 2),
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 1)
                // testHinban3 が不足 → 途中で停止
            },
            new List<(Hinban, int)> {
                (testHinban1, 1)
            }
        };
        yield return new object[] {
            // すでにすべての品番が積み込み済み
            new List<ShippingPalletItem> {
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban1, 0) { IsCompleted = true },
                new ShippingPalletItem(new ShippingPalletID("20250101", "N", 10), testHinban2, 1) { IsCompleted = true },
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 2),
                new LocalLoadableItem(testHinban2, 2)
            },
            new List<(Hinban, int)> { }
        };
    }

    private record LocalLoadableItem(Hinban Hinban, int Quantity) : ILoadableItem;
}
