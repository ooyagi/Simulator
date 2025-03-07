using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services.Tests;

public class TakeShippingPalletSelectorTest
{
    private static TakeShippingPalletSelector CreateService(
        IShikakariStorageLoader? shikakariStorageLoaderParam = null,
        ITempStorageLoader? tempStorageLoaderParam = null,
        IInventoryStorageLoader? inventoryStorageLoaderParam = null,
        IWorkOrderLoader? workOrderLoaderParam = null
    ) {
        var logger = NullLogger<TakeShippingPalletSelector>.Instance;
        var shikakariStorageLoader = shikakariStorageLoaderParam ?? ((Func<IShikakariStorageLoader>)(() => {
            var mock = new Mock<IShikakariStorageLoader>();
            return mock.Object;
        }))();
        var tempStorageLoader = tempStorageLoaderParam ?? ((Func<ITempStorageLoader>)(() => {
            var mock = new Mock<ITempStorageLoader>();
            mock.Setup(x => x.GetAvarableHinbans(It.IsAny<ShippingStationCode>())).Returns(Enumerable.Empty<IInventoryPalletInfo>());
            return mock.Object;
        }))();
        var inventoryStorageLoader = inventoryStorageLoaderParam ?? ((Func<IInventoryStorageLoader>)(() => {
            var mock = new Mock<IInventoryStorageLoader>();
            return mock.Object;
        }))();
        var workOrderLoader = workOrderLoaderParam ?? ((Func<IWorkOrderLoader>)(() => {
            var mock = new Mock<IWorkOrderLoader>();
            return mock.Object;
        }))();
        return new TakeShippingPalletSelector(
            logger,
            shikakariStorageLoader,
            tempStorageLoader,
            inventoryStorageLoader,
            workOrderLoader
        );
    }

    public class SelectTakeShippingPallet
    {
        [Theory]
        [InlineData("N")]
        [InlineData("A")]
        public void 一時置き場の在庫だけで完了する出荷パレットが1件の場合はそのパレットIDを返す(
            string expectedPalletGroup
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var testHinban = new Hinban("FA-1609P(8)BV/U61");
            var testLocationCode = new LocationCode("T01");
            var expectedPalletID = new ShippingPalletID("20250101", expectedPalletGroup, 10);
            var tempStorageItem = new AvalableHinban(testLocationCode, testHinban, 10);
            var completableShippingPallet = new TestCompletablePalletInfo(new LocationCode("SP01"), expectedPalletID, testHinban, 1);

            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { tempStorageItem });
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.FilterCompletableBy(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo> { completableShippingPallet });

            var service = CreateService(
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object,
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );

            var result = service.SelectTakeShippingPallet(stationCode);

            Assert.Equal(expectedPalletID, result);
        }
        [Theory]
        [InlineData(3, 1, 2)]
        [InlineData(1, 3, 1)]
        public void 一時置き場の在庫だけで完了する出荷パレット候補が複数ある場合はStepが最小の候補を返す(
            int step1,
            int step2,
            int expected
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var testLocationCode1 = new LocationCode("T01");
            var testLocationCode2 = new LocationCode("T02");
            var palletID1 = new ShippingPalletID("20250101", "N", 10);
            var palletID2 = new ShippingPalletID("20250101", "A", 10);
            var tempStorageItem1 = new AvalableHinban(testLocationCode1, testHinban1, 10);
            var tempStorageItem2 = new AvalableHinban(testLocationCode2, testHinban2, 10);
            var completableShippingPallet1 = new TestCompletablePalletInfo(testLocationCode1, palletID1, testHinban1, step1);
            var completableShippingPallet2 = new TestCompletablePalletInfo(testLocationCode2, palletID2, testHinban2, step2);

            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(new List<IInventoryPalletInfo> { tempStorageItem1, tempStorageItem2 });
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.FilterCompletableBy(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(new List<ICompletablePalletInfo> { completableShippingPallet1, completableShippingPallet2 });

            var service = CreateService(
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object,
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );

            var result = service.SelectTakeShippingPallet(stationCode);

            var expectedPalletID = expected == 1 ? palletID1 : palletID2;
            Assert.Equal(expectedPalletID, result);
        }
        [Fact]
        public void 使い切れる在庫パレットが1件の場合はそのパレットIDを返す() {
            var stationCode = new ShippingStationCode("ST01");
            var testHinban = new Hinban("FA-1609P(8)BV/U61");
            var tempLocation = new LocationCode("T01");
            var expectedPalletID = new ShippingPalletID("20250101", "N", 10);
            var tempStorageItem = new AvalableHinban(tempLocation, testHinban, 10);
            var tempStorageItems = new List<IInventoryPalletInfo> { tempStorageItem };
            var testEmptiableInfo = new TestEmptiablePalletInfo(new LocationCode("SP01"), testHinban, 10);
            var testEmptiableInfos = new List<TestEmptiablePalletInfo> { testEmptiableInfo };
            var shikakariLoadableHinbanInfoMock = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariLoadableHinbanInfoMock.Setup(x => x.ShippingPalletID).Returns(expectedPalletID);
            shikakariLoadableHinbanInfoMock.Setup(x => x.GetEmptiablePallets(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(testEmptiableInfos);

            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(tempStorageItems);
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.FilterCompletableBy(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShikakariPalletLoadableHinbanInfo> { shikakariLoadableHinbanInfoMock.Object });
            var service = CreateService(
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object,
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );

            var result = service.SelectTakeShippingPallet(stationCode);

            Assert.Equal(expectedPalletID, result);
        }
        [Theory]
        [InlineData(2, 4, 1)]
        [InlineData(5, 3, 2)]
        public void 積み切れる在庫パレット候補が複数ある場合はEmptiableStepが小さい方のパレットIDを返す(
            int step1,
            int step2,
            int expected
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var testHinban = new Hinban("FA-1609P(8)BV/U61");
            var tempLocation = new LocationCode("T01");
            var expectedPalletID1 = new ShippingPalletID("20250101", "N", 10);
            var expectedPalletID2 = new ShippingPalletID("20250101", "A", 10);
            var tempStorageItem = new AvalableHinban(tempLocation, testHinban, 10);
            var tempStorageItems = new List<IInventoryPalletInfo> { tempStorageItem };
            var testEmptiableInfo1 = new TestEmptiablePalletInfo(new LocationCode("SP01"), testHinban, step1);
            var testEmptiableInfo2 = new TestEmptiablePalletInfo(new LocationCode("SP02"), testHinban, step2);
            var shikakariLoadableHinbanInfoMock1 = new Mock<IShikakariPalletLoadableHinbanInfo>();

            shikakariLoadableHinbanInfoMock1.Setup(x => x.ShippingPalletID).Returns(expectedPalletID1);
            shikakariLoadableHinbanInfoMock1.Setup(x => x.GetEmptiablePallets(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IEmptiablePalletInfo>{ testEmptiableInfo1 });
            var shikakariLoadableHinbanInfoMock2 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariLoadableHinbanInfoMock2.Setup(x => x.ShippingPalletID).Returns(expectedPalletID2);
            shikakariLoadableHinbanInfoMock2.Setup(x => x.GetEmptiablePallets(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IEmptiablePalletInfo>{ testEmptiableInfo2 });
            var shippingPalletLoadableHinbanInfos = new List<IShikakariPalletLoadableHinbanInfo> { shikakariLoadableHinbanInfoMock1.Object, shikakariLoadableHinbanInfoMock2.Object };

            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(tempStorageItems);
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.FilterCompletableBy(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(new List<ICompletablePalletInfo>());
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shippingPalletLoadableHinbanInfos);
            var service = CreateService(
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object,
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );

            var result = service.SelectTakeShippingPallet(stationCode);

            ShippingPalletID expectedPalletID = expected == 1 ? expectedPalletID1 : expectedPalletID2;
            Assert.Equal(expectedPalletID, result);
        }
        [Theory]
        [InlineData(2, 4, 1)]
        [InlineData(5, 3, 2)]
        public void 一時置き場の在庫パレットの内_積み込み可能な仕掛パレット数が少ない品番が次回積込予定となっている出荷パレットが複数ある場合は利用予定の品番が少ないパレットIDを返す(
            int futureLoadableCount1,
            int futureLoadableCount2,
            int expected
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var palletID1 = new ShippingPalletID("20250101", "N", 10);
            var palletID2 = new ShippingPalletID("20250101", "A", 10);
            var tempStorageItem1 = new AvalableHinban(new LocationCode("T01"), testHinban1, 10);
            var tempStorageItem2 = new AvalableHinban(new LocationCode("T02"), testHinban2, 8);
            var tempStorageItems = new List<IInventoryPalletInfo> { tempStorageItem1, tempStorageItem2 };
        
            var shikakariPalletInfo1 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariPalletInfo1.Setup(x => x.NextHinban).Returns(testHinban1);
            shikakariPalletInfo1.Setup(x => x.ShippingPalletID).Returns(palletID1);
            shikakariPalletInfo1.Setup(x => x.FutureLoadableHinbanTypeCount).Returns(futureLoadableCount1);
            shikakariPalletInfo1.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban1, It.IsAny<int>())).Returns(true);
            shikakariPalletInfo1.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban2, It.IsAny<int>())).Returns(false);
            shikakariPalletInfo1.Setup(x => x.GetEmptiablePallets(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(Enumerable.Empty<IEmptiablePalletInfo>());
            var shikakariPalletInfo2 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariPalletInfo2.Setup(x => x.NextHinban).Returns(testHinban1);
            shikakariPalletInfo2.Setup(x => x.ShippingPalletID).Returns(palletID2);
            shikakariPalletInfo2.Setup(x => x.FutureLoadableHinbanTypeCount).Returns(futureLoadableCount2);
            shikakariPalletInfo2.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban1, It.IsAny<int>())).Returns(true);
            shikakariPalletInfo2.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban2, It.IsAny<int>())).Returns(false);
            shikakariPalletInfo2.Setup(x => x.GetEmptiablePallets(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(Enumerable.Empty<IEmptiablePalletInfo>());
            var shikakariPallets = new List<IShikakariPalletLoadableHinbanInfo> { shikakariPalletInfo1.Object, shikakariPalletInfo2.Object };
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(tempStorageItems);
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.FilterCompletableBy(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(new List<ICompletablePalletInfo>());
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shikakariPallets);
        
            var service = CreateService(
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object,
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );
        
            var result = service.SelectTakeShippingPallet(stationCode);
        
            ShippingPalletID expectedPalletID = expected == 1 ? palletID1 : palletID2;
            Assert.Equal(expectedPalletID, result);
        }
        [Theory]
        [InlineData(true, true, true, false, 2)]
        [InlineData(true, false, true, true, 1)]
        [InlineData(false, false, false, true, 1)]
        public void 一時置き場の在庫パレットの内_積み込み可能な仕掛パレット数が少ない品番が次回積込予定となっている出荷パレットが１つの場合は利用予定の品番にかかわらず該当パレットのパレットIDを返す(
            bool pallet1_hinban1_loadable,
            bool pallet2_hinban1_loadable,
            bool pallet1_hinban2_loadable,
            bool pallet2_hinban2_loadable,
            int expected
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var tempLocation1 = new LocationCode("T01");
            var tempLocation2 = new LocationCode("T02");
            var expectedPalletID1 = new ShippingPalletID("20250101", "N", 10);
            var expectedPalletID2 = new ShippingPalletID("20250101", "A", 10);
        
            var tempStorageItem1 = new AvalableHinban(tempLocation1, testHinban1, 10);
            var tempStorageItem2 = new AvalableHinban(tempLocation2, testHinban2, 8);
            var tempStorageItems = new List<IInventoryPalletInfo> { tempStorageItem1, tempStorageItem2 };
            var shikakariMock1 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariMock1.Setup(x => x.NextHinban).Returns(testHinban1);
            shikakariMock1.Setup(x => x.ShippingPalletID).Returns(expectedPalletID1);
            shikakariMock1.Setup(x => x.FutureLoadableHinbanTypeCount).Returns(5);
            shikakariMock1.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban1, It.IsAny<int>())).Returns(pallet1_hinban1_loadable);
            shikakariMock1.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban2, It.IsAny<int>())).Returns(pallet1_hinban2_loadable);
            shikakariMock1.Setup(x => x.GetEmptiablePallets(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(Enumerable.Empty<IEmptiablePalletInfo>());
            var shikakariMock2 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariMock2.Setup(x => x.NextHinban).Returns(testHinban2);
            shikakariMock2.Setup(x => x.ShippingPalletID).Returns(expectedPalletID2);
            shikakariMock2.Setup(x => x.FutureLoadableHinbanTypeCount).Returns(10);
            shikakariMock2.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban1, It.IsAny<int>())).Returns(pallet2_hinban1_loadable);
            shikakariMock2.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban2, It.IsAny<int>())).Returns(pallet2_hinban2_loadable);
            shikakariMock2.Setup(x => x.GetEmptiablePallets(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(Enumerable.Empty<IEmptiablePalletInfo>());
            var shikakariPallets = new List<IShikakariPalletLoadableHinbanInfo> { shikakariMock1.Object, shikakariMock2.Object };
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(tempStorageItems);
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.FilterCompletableBy(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(shikakariPallets);
        
            var service = CreateService(
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object,
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );
        
            var result = service.SelectTakeShippingPallet(stationCode);
        
            ShippingPalletID expectedPalletID = expected == 1 ? expectedPalletID1 : expectedPalletID2;
            Assert.Equal(expectedPalletID.Value, result?.Value);
        }
        [Theory]
        [InlineData(true, false, true, true, false, false, 1)]
        [InlineData(true, true, true, false, false, false, 2)]
        [InlineData(false, false, false, true, false, true, 1)]
        [InlineData(true, true, false, true, false, true, 2)]
        public void 一時置き場の在庫パレットの内_積み込み可能な仕掛パレット数が少ない品番が次回積込予定となっている出荷パレットを選択する際_次回積込品番に一致しない候補は無視される(
            bool pallet1_hinban1_loadable,
            bool pallet2_hinban1_loadable,
            bool pallet1_hinban2_loadable,
            bool pallet2_hinban2_loadable,
            bool pallet1_hinban3_loadable,
            bool pallet2_hinban3_loadable,
            int expected
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var testHinban3 = new Hinban("FA-1209P(5)NV/FG105");
            var tempLocation1 = new LocationCode("T01");
            var tempLocation2 = new LocationCode("T02");
            var expectedPalletID1 = new ShippingPalletID("20250101", "N", 10);
            var expectedPalletID2 = new ShippingPalletID("20250101", "A", 10);
        
            // testHinban3 は NextHinbanに指定されないため IsLoadableQuantityGreaterThan の戻りにかかわらず選択されないことを確認する
            // testHinban3 が最も積み込み可能な仕掛パレット数が少ないと判定され、testHinban3 が次回積込品番のパレットがないため「対象パレットなし」とならないことの確認
            var tempStorageItem1 = new AvalableHinban(tempLocation1, testHinban1, 10);
            var tempStorageItem2 = new AvalableHinban(tempLocation2, testHinban2, 8);
            var tempStorageItems = new List<IInventoryPalletInfo> { tempStorageItem1, tempStorageItem2 };
            var shikakariMock1 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariMock1.Setup(x => x.NextHinban).Returns(testHinban1);
            shikakariMock1.Setup(x => x.ShippingPalletID).Returns(expectedPalletID1);
            shikakariMock1.Setup(x => x.FutureLoadableHinbanTypeCount).Returns(5);
            shikakariMock1.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban1, It.IsAny<int>())).Returns(pallet1_hinban1_loadable);
            shikakariMock1.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban2, It.IsAny<int>())).Returns(pallet1_hinban2_loadable);
            shikakariMock1.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban3, It.IsAny<int>())).Returns(pallet1_hinban3_loadable);
            shikakariMock1.Setup(x => x.GetEmptiablePallets(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(Enumerable.Empty<IEmptiablePalletInfo>());
            var shikakariMock2 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariMock2.Setup(x => x.NextHinban).Returns(testHinban2);
            shikakariMock2.Setup(x => x.ShippingPalletID).Returns(expectedPalletID2);
            shikakariMock2.Setup(x => x.FutureLoadableHinbanTypeCount).Returns(10);
            shikakariMock2.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban1, It.IsAny<int>())).Returns(pallet2_hinban1_loadable);
            shikakariMock2.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban2, It.IsAny<int>())).Returns(pallet2_hinban2_loadable);
            shikakariMock2.Setup(x => x.IsLoadableQuantityGreaterThan(testHinban3, It.IsAny<int>())).Returns(pallet2_hinban3_loadable);
            shikakariMock2.Setup(x => x.GetEmptiablePallets(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(Enumerable.Empty<IEmptiablePalletInfo>());
            var shikakariPallets = new List<IShikakariPalletLoadableHinbanInfo> { shikakariMock1.Object, shikakariMock2.Object };
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(tempStorageItems);
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.FilterCompletableBy(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(shikakariPallets);
        
            var service = CreateService(
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object,
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );
        
            var result = service.SelectTakeShippingPallet(stationCode);
        
            ShippingPalletID expectedPalletID = expected == 1 ? expectedPalletID1 : expectedPalletID2;
            Assert.Equal(expectedPalletID.Value, result?.Value);
        }
    }

    public record TestCompletablePalletInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, int RemainStep): ICompletablePalletInfo;
    public record AvalableHinban(LocationCode LocationCode, Hinban Hinban, int Quantity): IInventoryPalletInfo;
    public record TestEmptiablePalletInfo(LocationCode LocationCode, Hinban Hinban, int EmptiableStep): IEmptiablePalletInfo;
    public record TestShikakariPalletLoadableHinbanInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, Hinban BlockHinban, int RemainStep, int FutureLoadableHinbanTypeCount, int BlockHinbanLoadableCount): IShikakariPalletLoadableHinbanInfo
    {
        public bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity) => false;
        public bool IsCompletableBy(IInventoryPalletInfo inventoryPallet) => false;
        public IEnumerable<IEmptiablePalletInfo> GetEmptiablePallets(IEnumerable<IInventoryPalletInfo> inventoryPallets) {
            return Enumerable.Empty<IEmptiablePalletInfo>();
        }
    }
}
