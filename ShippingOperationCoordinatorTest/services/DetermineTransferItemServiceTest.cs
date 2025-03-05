using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services.Tests;

public class DetermineTransferItemServiceTests
{
    private static DetermineTransferItemService CreateService(
        ShippingStationCode stationCode,
        ITempStorageLoader? tempStorageLoaderParam = null,
        IShippingStorageLoader? shippingStorageLoaderParam = null,
        IShikakariStorageLoader? ShikakariStorageLoaderParam = null
    ) {
        var logger = NullLogger<DetermineTransferItemService>.Instance;
        ITempStorageLoader tempStorageLoader = tempStorageLoaderParam
            ?? ((Func<ITempStorageLoader>)(() => {
                var mock = new Mock<ITempStorageLoader>();
                mock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(Enumerable.Empty<IInventoryPalletInfo>());
                return mock.Object;
            }))();
        IShippingStorageLoader shippingStorageLoader = shippingStorageLoaderParam
            ?? ((Func<IShippingStorageLoader>)(() => {
                var mock = new Mock<IShippingStorageLoader>();
                return mock.Object;
            }))();
        IShikakariStorageLoader ShikakariStorageLoader = ShikakariStorageLoaderParam
            ?? ((Func<IShikakariStorageLoader>)(() => {
                var mock = new Mock<IShikakariStorageLoader>();
                return mock.Object;
            }))();
        return new DetermineTransferItemService(
            logger,
            tempStorageLoader,
            shippingStorageLoader,
            ShikakariStorageLoader
        );
    }

    public class DetermineTransferHinbanTest
    {
        [Fact]
        public void 利用可能な品番が存在しない場合_デフォルトを返す() {
            ShippingStationCode stationCode = new ShippingStationCode("S01");

            var service = CreateService(stationCode);

            var result = service.DetermineTransferHinban(stationCode);

            Assert.Equal(Hinban.Default, result.Hinban);
        }
        [Fact]
        public void 出荷パレット完了候補が1つだけある場合_その候補を返す() {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban = new Hinban("FA-1609P(8)BV/U61");
            var testTempItem = new TestAvarableHinban(new LocationCode("T01"), testHinban, 10);
            var shippingPalletId = new ShippingPalletID("20250101", "N", 1);
    
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(new List<IInventoryPalletInfo> { testTempItem });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.FilterCompletableBy(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo> { new TestCompletablePalletInfo(new LocationCode("SP01"), shippingPalletId, testHinban, 1) });
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object
            );
    
            var result = service.DetermineTransferHinban(stationCode);

            Assert.Equal(testHinban, result.Hinban);
        }
        [Theory]
        [InlineData(3, 1, 2)]
        [InlineData(1, 3, 1)]
        [InlineData(2, 2, 1)]
        public void 出荷パレット完了候補が複数ある場合_積替えステップが最も小さい候補を返す(
            int step1,
            int step2,
            int expected
        ) {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var testTempPallet1 = new TestAvarableHinban(new LocationCode("T01"), testHinban1, 10);
            var testTempPallet2 = new TestAvarableHinban(new LocationCode("T02"), testHinban2, 10);
            var shippingPalletId1 = new ShippingPalletID("20250101", "N", 1);
            var shippingPalletId2 = new ShippingPalletID("20250101", "N", 2);
            var testShipPallet1 = new TestCompletablePalletInfo(new LocationCode("SP01"), shippingPalletId1, testHinban1, step1);
            var testShipPallet2 = new TestCompletablePalletInfo(new LocationCode("SP02"), shippingPalletId2, testHinban2, step2);
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { testTempPallet1, testTempPallet2 });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.FilterCompletableBy(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo> { testShipPallet1, testShipPallet2 });
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object
            );
        
            var result = service.DetermineTransferHinban(stationCode);
        
            Hinban expectedHinban = expected == 1 ? testHinban1 : testHinban2;
            LocationCode expectedFrom = expected == 1 ? testTempPallet1.LocationCode : testTempPallet2.LocationCode;
            LocationCode expectedTo = expected == 1 ? testShipPallet1.LocationCode : testShipPallet2.LocationCode;
            Assert.Equal(expectedHinban, result.Hinban);
            Assert.Equal(expectedFrom, result.From);
            Assert.Equal(expectedTo, result.To);
        }

        [Theory]
        [InlineData(1, true, false)]
        [InlineData(2, false, true)]
        public void 使い切れる在庫パレットが1つある場合_その在庫パレットを返す(
            int expected,
            bool isLoadableQuantityGreaterThanResult1,
            bool isLoadableQuantityGreaterThanResult2
        ) {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var testTempPallet1 = new TestAvarableHinban(new LocationCode("T01"), testHinban1, 10);
            var testTempPallet2 = new TestAvarableHinban(new LocationCode("T02"), testHinban2, 10);
            var testShipPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP01"), ShippingPalletID.CustomPaletteID, testHinban1, 4, 2, isLoadableQuantityGreaterThanResult1);
            var testShipPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP01"), ShippingPalletID.CustomPaletteID, testHinban2, 4, 2, isLoadableQuantityGreaterThanResult2);
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { testTempPallet1, testTempPallet2 });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.FilterCompletableBy(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShippingPalletLoadableHinbanInfo> { testShipPalletInfo1, testShipPalletInfo2 });
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object
            );
        
            var result = service.DetermineTransferHinban(stationCode);

            Hinban expectedHinban = expected == 1 ? testHinban1 : testHinban2;
            LocationCode exceptedFrom = expected == 1 ? testTempPallet1.LocationCode : testTempPallet2.LocationCode;
            LocationCode expectedTo = expected == 1 ? testShipPalletInfo1.LocationCode : testShipPalletInfo2.LocationCode;
            Assert.Equal(expectedHinban, result.Hinban);
            Assert.Equal(exceptedFrom, result.From);
            Assert.Equal(expectedTo, result.To);
        }
        [Theory]
        [InlineData(10, 14, 1)]
        [InlineData(14, 10, 2)]
        public void 使い切れる在庫パレットが複数ある場合_在庫数が少ないものを返す(
            int quantity1,
            int quantity2,
            int expected
        ) {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var testTempPallet1 = new TestAvarableHinban(new LocationCode("T01"), testHinban1, quantity1);
            var testTempPallet2 = new TestAvarableHinban(new LocationCode("T02"), testHinban2, quantity2);
            var testShipPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP01"), ShippingPalletID.CustomPaletteID, testHinban1, 4, 1, true);
            var testShipPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP02"), ShippingPalletID.CustomPaletteID, testHinban2, 4, 1, true);
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { testTempPallet1, testTempPallet2 });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.FilterCompletableBy(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShippingPalletLoadableHinbanInfo> { testShipPalletInfo1, testShipPalletInfo2 });
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object
            );
        
            var result = service.DetermineTransferHinban(stationCode);
        
            Hinban expectedHinban = expected == 1 ? testHinban1 : testHinban2;
            LocationCode expectedFrom = expected == 1 ? testTempPallet1.LocationCode : testTempPallet2.LocationCode;
            LocationCode expectedTo = expected == 1 ? testShipPalletInfo1.LocationCode : testShipPalletInfo2.LocationCode;
            Assert.Equal(expectedHinban, result.Hinban);
            Assert.Equal(expectedFrom, result.From);
            Assert.Equal(expectedTo, result.To);
        }
        [Theory]
        [InlineData(5, 3, 2)]
        [InlineData(2, 4, 1)]
        public void 使い切れる在庫パレットの積替え先の出荷パレットが複数ある場合_積替えステップが最も小さいものを選択する(
            int step1,
            int step2,
            int expected
        ) {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var testTempPallet1 = new TestAvarableHinban(new LocationCode("T01"), testHinban1, 8);
            var testTempPallet2 = new TestAvarableHinban(new LocationCode("T02"), testHinban2, 10);
            var testShipPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP01"), ShippingPalletID.CustomPaletteID, testHinban1, 4, step1, true);
            var testShipPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP02"), ShippingPalletID.CustomPaletteID, testHinban1, 4, step2, true);
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { testTempPallet1, testTempPallet2 });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.FilterCompletableBy(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShippingPalletLoadableHinbanInfo> { testShipPalletInfo1, testShipPalletInfo2 });
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object
            );
        
            var result = service.DetermineTransferHinban(stationCode);
        
            Hinban expectedHinban = testHinban1;
            LocationCode expectedFrom = testTempPallet1.LocationCode;
            LocationCode expectedTo = expected == 1 ? testShipPalletInfo1.LocationCode : testShipPalletInfo2.LocationCode;
            Assert.Equal(expectedHinban, result.Hinban);
            Assert.Equal(expectedFrom, result.From);
            Assert.Equal(expectedTo, result.To);
        }

        [Fact]
        public void 仕掛パレットで使用しない在庫パレットが1つの場合_その候補を返す() {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var testTempPallet1 = new TestAvarableHinban(new LocationCode("T01"), testHinban1, 10);
            var testTempPallet2 = new TestAvarableHinban(new LocationCode("T02"), testHinban2, 10);
            var testShipPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP01"), ShippingPalletID.CustomPaletteID, testHinban1, 0, 0, false);
            var testShipPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP02"), ShippingPalletID.CustomPaletteID, testHinban2, 0, 0, false);
            var testShikakariPalletInfo1 = new TestShikakariPalletLoadableHinbanInfo(new LocationCode("SK01"), ShippingPalletID.CustomPaletteID, testHinban1, Hinban.Default, 0, 0, true, 0);
            var testShikakariPalletInfo2 = new TestShikakariPalletLoadableHinbanInfo(new LocationCode("SK02"), ShippingPalletID.CustomPaletteID, testHinban2, Hinban.Default, 0, 0, false, 0);
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { testTempPallet1, testTempPallet2 });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.FilterCompletableBy(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShippingPalletLoadableHinbanInfo> { testShipPalletInfo1, testShipPalletInfo2 });
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShikakariPalletLoadableHinbanInfo> { testShikakariPalletInfo1, testShikakariPalletInfo2 });
        
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                ShikakariStorageLoaderParam: shikakariStorageLoaderMock.Object
            );
        
            var result = service.DetermineTransferHinban(stationCode);
        
            shikakariStorageLoaderMock.Verify(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()), Times.Once);
            Assert.Equal(testHinban2, result.Hinban);
            Assert.Equal(testTempPallet2.LocationCode, result.From);
            Assert.Equal(testShipPalletInfo2.LocationCode, result.To);
        }

        [Theory]
        [InlineData(10, 14, 1)]
        [InlineData(14, 10, 2)]
        public void 仕掛パレットで使用しない在庫パレットが複数ある場合_在庫数が少ないものを返す(
            int quantity1,
            int quantity2,
            int expected
        ) {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var testTempPallet1 = new TestAvarableHinban(new LocationCode("T01"), testHinban1, quantity1);
            var testTempPallet2 = new TestAvarableHinban(new LocationCode("T02"), testHinban2, quantity2);
            var testShipPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP01"), ShippingPalletID.CustomPaletteID, testHinban1, 0, 0, false);
            var testShipPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP02"), ShippingPalletID.CustomPaletteID, testHinban2, 0, 0, false);
            var testShikakariPalletInfo1 = new TestShikakariPalletLoadableHinbanInfo(new LocationCode("SK01"), ShippingPalletID.CustomPaletteID, testHinban1, Hinban.Default, 0, 0, false, 0);
            var testShikakariPalletInfo2 = new TestShikakariPalletLoadableHinbanInfo(new LocationCode("SK02"), ShippingPalletID.CustomPaletteID, testHinban2, Hinban.Default, 0, 0, false, 0);
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { testTempPallet1, testTempPallet2 });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.FilterCompletableBy(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShippingPalletLoadableHinbanInfo> { testShipPalletInfo1, testShipPalletInfo2 });
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShikakariPalletLoadableHinbanInfo> { testShikakariPalletInfo1, testShikakariPalletInfo2 });
        
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                ShikakariStorageLoaderParam: shikakariStorageLoaderMock.Object
            );
        
            var result = service.DetermineTransferHinban(stationCode);
        
            Hinban expectedHinban = expected == 1 ? testHinban1 : testHinban2;
            LocationCode expectedFrom = expected == 1 ? testTempPallet1.LocationCode : testTempPallet2.LocationCode;
            LocationCode expectedTo = expected == 1 ? testShipPalletInfo1.LocationCode : testShipPalletInfo2.LocationCode;
            shikakariStorageLoaderMock.Verify(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()), Times.Once);
            Assert.Equal(expectedHinban, result.Hinban);
            Assert.Equal(expectedFrom, result.From);
            Assert.Equal(expectedTo, result.To);
        }
        
        [Theory]
        [InlineData(5, 3, 2)]
        [InlineData(2, 4, 1)]
        public void 仕掛パレットで使用しない在庫パレットの積替え先の品番が複数ある場合_積替えステップが最も小さいものを選択する(
            int step1,
            int step2,
            int expected
        ) {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban = new Hinban("FA-1609P(8)BV/U61");
            var testTempPallet = new TestAvarableHinban(new LocationCode("T01"), testHinban, 10);
            var testShipPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(
                new LocationCode("SP01"), ShippingPalletID.CustomPaletteID, testHinban, 0, step1, false);
            var testShipPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(
                new LocationCode("SP02"), ShippingPalletID.CustomPaletteID, testHinban, 0, step2, false);
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { testTempPallet });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShippingPalletLoadableHinbanInfo> { testShipPalletInfo1, testShipPalletInfo2 });
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            // 仕掛パレットが無い場合も仕掛パレットで使用しないと言う判断になる
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShikakariPalletLoadableHinbanInfo> {});
        
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                ShikakariStorageLoaderParam: shikakariStorageLoaderMock.Object
            );
        
            var result = service.DetermineTransferHinban(stationCode);
        
            Hinban expectedHinban = testHinban;
            LocationCode expectedFrom = testTempPallet.LocationCode;
            LocationCode expectedTo = expected == 1 ? testShipPalletInfo1.LocationCode : testShipPalletInfo2.LocationCode;
            shikakariStorageLoaderMock.Verify(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()), Times.Once);
            Assert.Equal(expectedHinban, result.Hinban);
            Assert.Equal(expectedFrom, result.From);
            Assert.Equal(expectedTo, result.To);
        }
        [Theory]
        [InlineData(5, 3, 2)]
        [InlineData(2, 4, 1)]
        public void 他の条件に当てはまらず_積替え可能な回数が最も少ない出荷パレットが1つの場合_その候補を返す(
            int step1,
            int step2,
            int expected
        ) {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var testTempPallet1 = new TestAvarableHinban(new LocationCode("T01"), testHinban1, 14);
            var testTempPallet2 = new TestAvarableHinban(new LocationCode("T02"), testHinban2, 14);
            var testShipPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP01"), ShippingPalletID.CustomPaletteID, testHinban1, step1, 0, false);
            var testShipPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(new LocationCode("SP02"), ShippingPalletID.CustomPaletteID, testHinban2, step2, 0, false);
            var testShikakariPalletInfo1 = new TestShikakariPalletLoadableHinbanInfo(new LocationCode("SK01"), ShippingPalletID.CustomPaletteID, testHinban1, Hinban.Default, 0, 0, true, 0);
            var testShikakariPalletInfo2 = new TestShikakariPalletLoadableHinbanInfo(new LocationCode("SK02"), ShippingPalletID.CustomPaletteID, testHinban2, Hinban.Default, 0, 0, true, 0);
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { testTempPallet1, testTempPallet2 });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.FilterCompletableBy(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShippingPalletLoadableHinbanInfo> { testShipPalletInfo1, testShipPalletInfo2 });
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShikakariPalletLoadableHinbanInfo> { testShikakariPalletInfo1, testShikakariPalletInfo2 });
        
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                ShikakariStorageLoaderParam: shikakariStorageLoaderMock.Object
            );
        
            var result = service.DetermineTransferHinban(stationCode);
            Hinban expectedHinban = expected == 1 ? testHinban1 : testHinban2;
            LocationCode expectedFrom = expected == 1 ? testTempPallet1.LocationCode : testTempPallet2.LocationCode;
            LocationCode expectedTo = expected == 1 ? testShipPalletInfo1.LocationCode : testShipPalletInfo2.LocationCode;
            shippingStorageLoaderMock.Verify(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()), Times.Exactly(3));
            Assert.Equal(expectedHinban, result.Hinban);
            Assert.Equal(expectedFrom, result.From);
            Assert.Equal(expectedTo, result.To);
        }
        [Theory]
        [InlineData(5, 3, 2)]
        [InlineData(2, 4, 1)]
        public void 積替え可能な回数が同じ出荷パレットが複数ある場合_積替えステップが最も小さいものを選択する(
            int step1,
            int step2,
            int expected
        ) {
            ShippingStationCode stationCode = new ShippingStationCode("S01");
            var testHinban = new Hinban("FA-1609P(8)BV/U61");
            var testTempPallet = new TestAvarableHinban(new LocationCode("T01"), testHinban, 10);
            var testShipPalletInfo1 = new TestShippingPalletLoadableHinbanInfo(
                new LocationCode("SP01"), ShippingPalletID.CustomPaletteID, testHinban, 4, step1, false);
            var testShipPalletInfo2 = new TestShippingPalletLoadableHinbanInfo(
                new LocationCode("SP02"), ShippingPalletID.CustomPaletteID, testHinban, 4, step2, false);
            var testShikakariPalletInfo1 = new TestShikakariPalletLoadableHinbanInfo(new LocationCode("SK01"), ShippingPalletID.CustomPaletteID, testHinban, Hinban.Default, 0, 0, true, 0);
            
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode))
                .Returns(new List<IInventoryPalletInfo> { testTempPallet });
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.FilterCompletableBy(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<ICompletablePalletInfo>());
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShippingPalletLoadableHinbanInfo> { testShipPalletInfo1, testShipPalletInfo2 });
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>()))
                .Returns(new List<IShikakariPalletLoadableHinbanInfo> { testShikakariPalletInfo1 });
            
            var service = CreateService(
                stationCode,
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                ShikakariStorageLoaderParam: shikakariStorageLoaderMock.Object
            );
            
            var result = service.DetermineTransferHinban(stationCode);
            
            LocationCode expectedTo = expected == 1 ? testShipPalletInfo1.LocationCode : testShipPalletInfo2.LocationCode;
            shippingStorageLoaderMock.Verify(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>()), Times.Exactly(3));
            Assert.Equal(testHinban, result.Hinban);
            Assert.Equal(testTempPallet.LocationCode, result.From);
            Assert.Equal(expectedTo, result.To);
        }
    }
}

public record TestAvarableHinban(LocationCode LocationCode, Hinban Hinban, int Quantity): IInventoryPalletInfo;
public record TestCompletablePalletInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, int Step): ICompletablePalletInfo;
public record TestShippingPalletLoadableHinbanInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, int LoadableItemCount, int Step, bool IsLoadableQuantityGreaterThanResult): IShippingPalletLoadableHinbanInfo
{
    // 実際にはNextHinbanは IsLoadableQuantityGreaterThan の判定に使用しないが、テストの際に複数の候補がある場合にどの候補が選ばれるかを確認するために使用する
    public bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity) => hinban == NextHinban && IsLoadableQuantityGreaterThanResult;
}
public record TestShikakariPalletLoadableHinbanInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, Hinban BlockHinban, int FutureLoadableHinbanTypeCount, int RemainStep, bool IsLoadableQuantityGreaterThanResult, int BlockHinbanLoadableCount): IShikakariPalletLoadableHinbanInfo
{
    // 実際にはNextHinbanは IsLoadableQuantityGreaterThan の判定に使用しないが、テストの際に複数の候補がある場合にどの候補が選ばれるかを確認するために使用する
    public bool IsLoadableQuantityGreaterThan(Hinban hinban, int quantity) => hinban == NextHinban && IsLoadableQuantityGreaterThanResult;
    public IEnumerable<IEmptiablePalletInfo> GetEmptiablePallets(IEnumerable<IInventoryPalletInfo> inventoryPallets) {
        return new List<IEmptiablePalletInfo>();
    }
}
