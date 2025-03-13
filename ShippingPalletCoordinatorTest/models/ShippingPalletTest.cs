using CommonItems.Models;
using ShippingPalletCoordinator.Models;

public class ShippingPalletTests
{
    private static readonly Hinban testHinban1 = new Hinban("FA-1609P(8)BV/U61");
    private static readonly Hinban testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
    private static readonly Hinban testHinban3 = new Hinban("FA-1209P(5)NV/FG105");
    private static readonly ShippingPalletID palletID1 = new ShippingPalletID("20250101", "N", 10);
    private static readonly ShippingPalletID palletID2 = new ShippingPalletID("20250101", "A", 10);

    [Theory]
    [MemberData(nameof(GetStepToCompletionTestData))]
    void GetStepToCompletion_ReturnsExpectedSteps(
        IEnumerable<ShippingPalletItem> palletItems,
        IEnumerable<ILoadableItem> loadableItems,
        int expectedSteps
    ) {
        var pallet = new ShippingPallet(palletID1, 0, palletItems);
        var result = pallet.GetStepToCompletion(loadableItems);

        Assert.Equal(expectedSteps, result);
    }

    public static IEnumerable<object[]> GetStepToCompletionTestData() {
        yield return new object[] {
            // 完全に積み込み可能なケース
            new List<ShippingPalletItem> {
                new ShippingPalletItem(palletID1, testHinban1, 0),
                new ShippingPalletItem(palletID1, testHinban2, 1),
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 1),
                new LocalLoadableItem(testHinban2, 1)
            },
            2 // 2ステップで完了
        };
        yield return new object[] {
            // 一部積み込めないケース
            new List<ShippingPalletItem> {
                new ShippingPalletItem(palletID1, testHinban1, 0),
                new ShippingPalletItem(palletID1, testHinban3, 1),
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 1)
                // testHinban3 が不足しているため完了できない
            },
            -1 // 途中で積み込めなくなる
        };
        yield return new object[] {
            // 余分なアイテムがあっても積み込み可能なケース
            new List<ShippingPalletItem> {
                new ShippingPalletItem(palletID1, testHinban1, 0),
                new ShippingPalletItem(palletID1, testHinban2, 1),
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 2),
                new LocalLoadableItem(testHinban2, 2),
                new LocalLoadableItem(testHinban3, 3) // 余分な品番
            },
            2 // 必要な分が揃っているので 2ステップで完了
        };
        yield return new object[] {
            // すでにすべてのアイテムが積み込み済み
            new List<ShippingPalletItem> {
                new ShippingPalletItem(palletID1, testHinban1, 0) { IsCompleted = true },
                new ShippingPalletItem(palletID1, testHinban2, 1) { IsCompleted = true },
            },
            new List<ILoadableItem> {
                new LocalLoadableItem(testHinban1, 1),
                new LocalLoadableItem(testHinban2, 1)
            },
            0 // すでに完了しているので 0 ステップ
        };
    }

    private record LocalLoadableItem(Hinban Hinban, int Quantity): ILoadableItem;
}
