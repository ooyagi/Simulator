using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services.Tests;

public class TakeInventoryPalletSelectorTest
{
    private static TakeInventoryPalletSelector CreateService(
        IShippingStorageLoader? shippingStorageLoaderParam = null,
        IShikakariStorageLoader? shikakariStorageLoaderParam = null,
        ITempStorageLoader? tempStorageLoaderParam = null,
        IInventoryStorageLoader? inventoryStorageLoaderParam = null
    ) {
        var logger = NullLogger<TakeInventoryPalletSelector>.Instance;
        var shippingStorageLoader = shippingStorageLoaderParam ?? ((Func<IShippingStorageLoader>)(() => {
            var mock = new Mock<IShippingStorageLoader>();
            mock.Setup(x => x.GetLoadableFrom(It.IsAny<ShippingStationCode>(), It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(Enumerable.Empty<IShippingPalletLoadableHinbanInfo>());
            return mock.Object;
        }))();
        var shikakariStorageLoader = shikakariStorageLoaderParam ?? ((Func<IShikakariStorageLoader>)(() => {
            var mock = new Mock<IShikakariStorageLoader>();
            mock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(Enumerable.Empty<IShikakariPalletLoadableHinbanInfo>());
            return mock.Object;
        }))();
        var tempStorageLoader = tempStorageLoaderParam ?? ((Func<ITempStorageLoader>)(() => {
            var mock = new Mock<ITempStorageLoader>();
            mock.Setup(x => x.GetAvarableHinbans(It.IsAny<ShippingStationCode>())).Returns(Enumerable.Empty<IInventoryPalletInfo>());
            return mock.Object;
        }))();
        var inventoryStorageLoader = inventoryStorageLoaderParam ?? ((Func<IInventoryStorageLoader>)(() => {
            var mock = new Mock<IInventoryStorageLoader>();
            mock.Setup(x => x.GetStoragedItems()).Returns(Enumerable.Empty<IInventoryPalletInfo>());
            return mock.Object;
        }))();
        return new TakeInventoryPalletSelector(
            logger,
            shippingStorageLoader,
            shikakariStorageLoader,
            tempStorageLoader,
            inventoryStorageLoader
        );
    }

    public class SelectTakeInventoryPallet
    {
        ShippingStationCode stationCode = new ShippingStationCode("ST01");
        Hinban testHinban1 = new Hinban("FA-1609P(8)BV/U61");
        Hinban testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
        Hinban testHinban3 = new Hinban("FA-1209P(5)NV/FG105");
        ShippingPalletID palletID1 = new ShippingPalletID("20250101", "N", 10);
        ShippingPalletID palletID2 = new ShippingPalletID("20250101", "A", 10);

        [Theory]
        [InlineData(5, 10, 11, 1)]
        [InlineData(10, 5, 11, 2)]
        [InlineData(5, 10, 1, 1)]   // 出荷パレットが品番 1, 2のみのため, 3は無視される
        public void ブロック要因で絞り込んだ在庫パレットが複数存在する場合は在庫数が少ないもののHinbanが返される_出荷パレットの次回積込品番でフィルタリング(
            int hinban1Quantity,
            int hinban2Quantity,
            int hinban3Quantity,
            int expected
        ) {
            var inventoryItem1 = new TestInventoryPalletInfo(LocationCode.Default, testHinban1, hinban1Quantity);
            var inventoryItem2 = new TestInventoryPalletInfo(LocationCode.Default, testHinban2, hinban2Quantity);
            var inventoryItem3 = new TestInventoryPalletInfo(LocationCode.Default, testHinban3, hinban3Quantity);
            var shippingPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(LocationCode.Default, palletID1, Hinban.Default, testHinban1, 1, 1, 1);
            var shippingPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(LocationCode.Default, palletID2, Hinban.Default, testHinban2, 1, 1, 1);

            var inventoryItems = new List<IInventoryPalletInfo> { inventoryItem1, inventoryItem2, inventoryItem3 };
            var shippingPalletLoadableInfos = new List<IShippingPalletLoadableHinbanInfo> { shippingPalletInfo1, shippingPalletInfo2 };
        
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shippingPalletLoadableInfos);
            var inventoryStorageLoaderMock = new Mock<IInventoryStorageLoader>();
            inventoryStorageLoaderMock.Setup(x => x.GetStoragedItems()).Returns(inventoryItems);
            var service = CreateService(
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                inventoryStorageLoaderParam: inventoryStorageLoaderMock.Object
            );
        
            var resultHinban = service.SelectTakeInventoryPallet(stationCode);

            var expectedHinban = expected switch { 1 => testHinban1, 2 => testHinban2, 3 => testHinban3, _ => Hinban.Default };
            Assert.Equal(expectedHinban.Value, resultHinban?.Value);
        }
        [Theory]
        [InlineData(5, 10, 11, 1)]
        [InlineData(5, 10, 1, 1)]   // 出荷パレットが品番 1, 2のみのため, 3は無視される
        public void ブロック要因で絞り込んだ在庫パレットが複数存在する場合は在庫数が少ないもののHinbanが返される_出荷パレットのブロック品番でフィルタリング(
            int hinban1Quantity,
            int hinban2Quantity,
            int hinban3Quantity,
            int expected
        ) {
            var inventoryItem1 = new TestInventoryPalletInfo(LocationCode.Default, testHinban1, hinban1Quantity);
            var inventoryItem2 = new TestInventoryPalletInfo(LocationCode.Default, testHinban2, hinban2Quantity);
            var inventoryItem3 = new TestInventoryPalletInfo(LocationCode.Default, testHinban3, hinban3Quantity);
            var shippingPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(LocationCode.Default, palletID1, testHinban1, testHinban1, 1, 1, 1);
            var shippingPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(LocationCode.Default, palletID2, testHinban2, testHinban2, 1, 1, 1);

            var inventoryItems = new List<IInventoryPalletInfo> { inventoryItem1, inventoryItem2, inventoryItem3 };
            var shippingPalletLoadableInfos = new List<IShippingPalletLoadableHinbanInfo> { shippingPalletInfo1, shippingPalletInfo2 };
        
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shippingPalletLoadableInfos);
            var inventoryStorageLoaderMock = new Mock<IInventoryStorageLoader>();
            inventoryStorageLoaderMock.Setup(x => x.GetStoragedItems()).Returns(inventoryItems);
            var service = CreateService(
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                inventoryStorageLoaderParam: inventoryStorageLoaderMock.Object
            );
        
            var resultHinban = service.SelectTakeInventoryPallet(stationCode);

            var expectedHinban = expected switch { 1 => testHinban1, 2 => testHinban2, _ => Hinban.Default };
            Assert.Equal(expectedHinban.Value, resultHinban?.Value);
        }
        [Theory]
        [InlineData(true, true, false, true, 1)]
        [InlineData(true, false, true, true, 2)]
        [InlineData(false, false, false, true, 2)]
        public void 完了できる出荷パレット数が多い在庫パレットが選ばれる場合はその品番が返される(
            bool pallet1_hinban1_completable,
            bool pallet2_hinban1_completable,
            bool pallet1_hinban2_completable,
            bool pallet2_hinban2_completable,
            int expected
        ) {
            var inventoryItem1 = new TestInventoryPalletInfo(LocationCode.Default, testHinban1, 10);
            var inventoryItem2 = new TestInventoryPalletInfo(LocationCode.Default, testHinban2, 5);
            var shippingPalletLoadableInfo1 = new Mock<IShippingPalletLoadableHinbanInfo>();
            shippingPalletLoadableInfo1.Setup(x => x.NextHinban).Returns(testHinban1);
            shippingPalletLoadableInfo1.Setup(x => x.IsCompletableBy(It.IsAny<IInventoryPalletInfo>()))
                .Returns((IInventoryPalletInfo item) => item.Hinban == testHinban1 ? pallet1_hinban1_completable : pallet1_hinban2_completable);
            var shippingPalletLoadableInfo2 = new Mock<IShippingPalletLoadableHinbanInfo>();
            shippingPalletLoadableInfo2.Setup(x => x.NextHinban).Returns(testHinban2);
            shippingPalletLoadableInfo2.Setup(x => x.IsCompletableBy(It.IsAny<IInventoryPalletInfo>()))
                .Returns((IInventoryPalletInfo item) => item.Hinban == testHinban1 ? pallet2_hinban1_completable : pallet2_hinban2_completable);

            var inventoryItems = new List<IInventoryPalletInfo> { inventoryItem1, inventoryItem2 };
            var shippingPalletLoadableInfos = new List<IShippingPalletLoadableHinbanInfo> { shippingPalletLoadableInfo1.Object, shippingPalletLoadableInfo2.Object };
        
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(shippingPalletLoadableInfos);
            var inventoryStorageLoaderMock = new Mock<IInventoryStorageLoader>();
            inventoryStorageLoaderMock.Setup(x => x.GetStoragedItems())
                .Returns(inventoryItems);
            var service = CreateService(
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                inventoryStorageLoaderParam: inventoryStorageLoaderMock.Object
            );

            var resultHinban = service.SelectTakeInventoryPallet(stationCode);
        
            var expectedHinban = expected switch { 1 => testHinban1, 2 => testHinban2, 3 => testHinban3, _ => Hinban.Default };
            Assert.Equal(expectedHinban.Value, resultHinban?.Value);
        }
        [Theory]
        [InlineData(true, true, false, true, 1)]
        [InlineData(true, false, true, true, 2)]
        [InlineData(false, false, false, true, 2)]
        public void 完了できる仕掛パレット数が多い在庫パレットが選ばれる場合はその品番が返される(
            bool pallet1_hinban1_completable,
            bool pallet2_hinban1_completable,
            bool pallet1_hinban2_completable,
            bool pallet2_hinban2_completable,
            int expected
        ) {
            var inventoryItem1 = new TestInventoryPalletInfo(LocationCode.Default, testHinban1, 10);
            var inventoryItem2 = new TestInventoryPalletInfo(LocationCode.Default, testHinban2, 5);
            var shikakariPalletLoadableInfo1 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariPalletLoadableInfo1.Setup(x => x.NextHinban).Returns(testHinban1);
            shikakariPalletLoadableInfo1.Setup(x => x.IsCompletableBy(It.IsAny<IInventoryPalletInfo>()))
                .Returns((IInventoryPalletInfo item) => item.Hinban == testHinban1 ? pallet1_hinban1_completable : pallet1_hinban2_completable);
            var shikakariPalletLoadableInfo2 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariPalletLoadableInfo2.Setup(x => x.NextHinban).Returns(testHinban2);
            shikakariPalletLoadableInfo2.Setup(x => x.IsCompletableBy(It.IsAny<IInventoryPalletInfo>()))
                .Returns((IInventoryPalletInfo item) => item.Hinban == testHinban1 ? pallet2_hinban1_completable : pallet2_hinban2_completable);

            var inventoryItems = new List<IInventoryPalletInfo> { inventoryItem1, inventoryItem2 };
            var shikakariPalletLoadableInfos = new List<IShikakariPalletLoadableHinbanInfo> { shikakariPalletLoadableInfo1.Object, shikakariPalletLoadableInfo2.Object };
        
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shikakariPalletLoadableInfos);
            var inventoryStorageLoaderMock = new Mock<IInventoryStorageLoader>();
            inventoryStorageLoaderMock.Setup(x => x.GetStoragedItems()).Returns(inventoryItems);
            var service = CreateService(
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object,
                inventoryStorageLoaderParam: inventoryStorageLoaderMock.Object
            );

            var resultHinban = service.SelectTakeInventoryPallet(stationCode);
        
            var expectedHinban = expected switch { 1 => testHinban1, 2 => testHinban2, 3 => testHinban3, _ => Hinban.Default };
            Assert.Equal(expectedHinban.Value, resultHinban?.Value);
        }
        [Theory]
        [InlineData(1, 1, 1, 2, 1)]
        [InlineData(1, 3, 2, 2, 2)]
        [InlineData(1, 2, 1, 1, 1)]
        public void ブロック品番数の合計が多い在庫パレットが選ばれる(
            int shippingPallet1BlockHinban,
            int shippingPallet2BlockHinban,
            int shikakariPallet1BlockHinban,
            int shikakariPallet2BlockHinban,
            int expected
        )
        {
            var hinbanConverter = (int type) => type switch { 1 => testHinban1, 2 => testHinban2, 3 => testHinban3, _ => Hinban.Default };
            var inventoryItem1 = new TestInventoryPalletInfo(new LocationCode("I01"), testHinban1, 10);
            var inventoryItem2 = new TestInventoryPalletInfo(new LocationCode("I02"), testHinban2, 8);
            var shippingMock1 = new Mock<IShippingPalletLoadableHinbanInfo>();
            shippingMock1.Setup(x => x.BlockHinban).Returns((() => shippingPallet1BlockHinban == 1 ? testHinban1 : testHinban2));
            shippingMock1.Setup(x => x.IsCompletableBy(It.IsAny<IInventoryPalletInfo>())).Returns(false);
            var shippingMock2 = new Mock<IShippingPalletLoadableHinbanInfo>();
            shippingMock2.Setup(x => x.BlockHinban).Returns((() => shippingPallet2BlockHinban == 1 ? testHinban1 : testHinban2));
            shippingMock2.Setup(x => x.IsCompletableBy(It.IsAny<IInventoryPalletInfo>())).Returns(false);
            var shikakariMock1 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariMock1.Setup(x => x.BlockHinban).Returns((() => shikakariPallet1BlockHinban == 1 ? testHinban1 : testHinban2));
            shikakariMock1.Setup(x => x.IsCompletableBy(It.IsAny<IInventoryPalletInfo>())).Returns(false);
            var shikakariMock2 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariMock2.Setup(x => x.BlockHinban).Returns((() => shikakariPallet2BlockHinban == 1 ? testHinban1 : testHinban2));
            shikakariMock2.Setup(x => x.IsCompletableBy(It.IsAny<IInventoryPalletInfo>())).Returns(false);
        
            var inventoryItems = new List<IInventoryPalletInfo> { inventoryItem1, inventoryItem2 };
            var shippingPalletLoadableInfos = new List<IShippingPalletLoadableHinbanInfo> { shippingMock1.Object, shippingMock2.Object };
            var shikakariPalletLoadableInfos = new List<IShikakariPalletLoadableHinbanInfo> { shikakariMock1.Object, shikakariMock2.Object };
        
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<ShippingStationCode>(), It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shippingPalletLoadableInfos);
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shikakariPalletLoadableInfos);
            var inventoryStorageLoaderMock = new Mock<IInventoryStorageLoader>();
            inventoryStorageLoaderMock.Setup(x => x.GetStoragedItems()).Returns(inventoryItems);
            var service = CreateService(
                inventoryStorageLoaderParam: inventoryStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object
            );
        
            var resultHinban = service.SelectTakeInventoryPallet(stationCode);
        
            var expectedHinban = expected switch { 1 => testHinban1, 2 => testHinban2, _ => Hinban.Default };
            Assert.Equal(expectedHinban.Value, resultHinban?.Value);
        }
    }

    public record TestInventoryPalletInfo(LocationCode LocationCode, Hinban Hinban, int Quantity): IInventoryPalletInfo;
    public record TestShippingPalletLoadableHinbanInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, Hinban BlockHinban, int RemainStep, int RequiredHinbanTypeCount, int BlockHinbanLoadableCount): IShippingPalletLoadableHinbanInfo
    {
        public bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity) => false;
        public bool IsCompletableBy(IInventoryPalletInfo inventoryPallet) => false;
    }
    public record TestShikakariPalletLoadableHinbanInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, Hinban BlockHinban, int RemainStep, int RequiredHinbanTypeCount, int BlockHinbanLoadableCount, bool IsLoadable): IShikakariPalletLoadableHinbanInfo
    {
        public bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity) => false;
        public bool IsCompletableBy(IInventoryPalletInfo inventoryPallet) => false;
        public IEnumerable<IEmptiablePalletInfo> GetEmptiablePallets(IEnumerable<IInventoryPalletInfo> inventoryPallets) {
            return Enumerable.Empty<IEmptiablePalletInfo>();
        }
    }
}
