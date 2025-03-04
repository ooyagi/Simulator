using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services.Tests;


public class ReturnShippingPalletSelectorTest
{
    private static ReturnShippingPalletSelector CreateService(
        IShippingStorageLoader? shippingStorageLoaderParam = null,
        ITempStorageLoader? TempStorageLoaderParam = null
    ) {
        var logger = new NullLogger<ReturnShippingPalletSelector>();

        var shippingStorageLoader = shippingStorageLoaderParam ?? ((Func<IShippingStorageLoader>)(() => {
            var mock = new Mock<IShippingStorageLoader>();
            mock.Setup(m => m.All(It.IsAny<ShippingStationCode>())).Returns(new List<IShippingPalletInfo>());
            return mock.Object;
        }))();
        var TempStorageLoader = TempStorageLoaderParam ?? ((Func<ITempStorageLoader>)(() => {
            var mock = new Mock<ITempStorageLoader>();
            mock.Setup(m => m.IsPickable(It.IsAny<ShippingStationCode>(), It.IsAny<Hinban>(), 1)).Returns(true);
            return mock.Object;
        }))();
        return new ReturnShippingPalletSelector(logger, TempStorageLoader, shippingStorageLoader);
    }

    public class SelectReturnShippingPalletTests
    {
        [Fact]
        public void 返却可能なパレットが存在しない場合はNullを返す() {
            var stationCode = new ShippingStationCode("ST01");
            var location1 = new LocationCode("SP01");
            var location2 = new LocationCode("SP02");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var shippingPallets = new List<IShippingPalletInfo> {
                new TestShippingPalletInfo(location1, ShippingPalletID.CustomPaletteID, testHinban1, false),
                new TestShippingPalletInfo(location2, ShippingPalletID.CustomPaletteID, testHinban2, false)
            };
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(m => m.All(It.IsAny<ShippingStationCode>())).Returns(shippingPallets);
            var selector = CreateService(shippingStorageLoaderMock.Object);

            var result = selector.SelectReturnShippingPallet(stationCode);

            Assert.Null(result);
        }
        [Theory]
        [InlineData(false, true, 2)]
        [InlineData(true, false, 1)]
        public void 完了フラグがオンの出荷パレットが存在する場合はそのパレットのロケーションを返す(
            bool isCompleted1,
            bool isCompleted2,
            int expected
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var location1 = new LocationCode("SP01");
            var location2 = new LocationCode("SP02");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var shippingPallets = new List<IShippingPalletInfo> {
                new TestShippingPalletInfo(location1, ShippingPalletID.CustomPaletteID, testHinban1, isCompleted1),
                new TestShippingPalletInfo(location2, ShippingPalletID.CustomPaletteID, testHinban2, isCompleted2)
            };
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(m => m.All(It.IsAny<ShippingStationCode>())).Returns(shippingPallets);
            var selector = CreateService(shippingStorageLoaderMock.Object);

            var result = selector.SelectReturnShippingPallet(stationCode);

            var expectedLocation = expected == 1 ? location1 : location2;
            Assert.Equal(expectedLocation, result);
        }

        [Theory]
        [InlineData(false, true, 1)]
        [InlineData(true, false, 2)]
        public void 完了フラグがオンの出荷パレットが存在しないが在庫問い合わせで返却対象が見つかる場合はそのロケーションを返す(
            bool IsPickable1,
            bool IsPickable2,
            int expected
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var location1 = new LocationCode("SP01");
            var location2 = new LocationCode("SP02");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var shippingPallets = new List<IShippingPalletInfo> {
                new TestShippingPalletInfo(location1, ShippingPalletID.CustomPaletteID, testHinban1, false),
                new TestShippingPalletInfo(location2, ShippingPalletID.CustomPaletteID, testHinban2, false)
            };
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(m => m.All(It.IsAny<ShippingStationCode>())).Returns(shippingPallets);
            var TempStorageLoaderMock = new Mock<ITempStorageLoader>();
            TempStorageLoaderMock.Setup(m => m.IsPickable(It.IsAny<ShippingStationCode>(), testHinban1, It.IsAny<int>())).Returns(IsPickable1);
            TempStorageLoaderMock.Setup(m => m.IsPickable(It.IsAny<ShippingStationCode>(), testHinban2, It.IsAny<int>())).Returns(IsPickable2);
            var selector = CreateService(shippingStorageLoaderMock.Object, TempStorageLoaderMock.Object);

            var result = selector.SelectReturnShippingPallet(stationCode);

            var expectedLocation = expected == 1 ? location1 : location2;
            Assert.Equal(expectedLocation, result);
        }
    }
}
public record TestShippingPalletInfo(LocationCode LocationCode, ShippingPalletID ShippingPalletID, Hinban NextHinban, bool IsCompleted): IShippingPalletInfo;
