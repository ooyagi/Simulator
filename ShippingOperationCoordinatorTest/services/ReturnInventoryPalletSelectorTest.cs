using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services.Tests;


public class ReturnInventoryPalletSelectorTest
{
    private static ReturnInventoryPalletSelector CreateService(
        IShippingStorageLoader? shippingStorageLoaderParam = null,
        IShikakariStorageLoader? shikakariStorageLoaderParam = null,
        ITempStorageLoader? tempStorageLoaderParam = null
    ) {
        var logger = new NullLogger<ReturnInventoryPalletSelector>();

        var shippingStorageLoader = shippingStorageLoaderParam ?? ((Func<IShippingStorageLoader>)(() => {
            var mock = new Mock<IShippingStorageLoader>();
            mock.Setup(m => m.GetLoadableFrom(It.IsAny<ShippingStationCode>(), It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(new List<IShippingPalletLoadableHinbanInfo>());
            return mock.Object;
        }))();
        var shikakariStorageLoader = shikakariStorageLoaderParam ?? ((Func<IShikakariStorageLoader>)(() => {
            var mock = new Mock<IShikakariStorageLoader>();
            mock.Setup(m => m.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(new List<IShikakariPalletLoadableHinbanInfo>());
            return mock.Object;
        }))();
        var tempStorageLoader = tempStorageLoaderParam ?? ((Func<ITempStorageLoader>)(() => {
            var mock = new Mock<ITempStorageLoader>();
            mock.Setup(m => m.GetEmptyPallets(It.IsAny<ShippingStationCode>())).Returns(new List<IInventoryPalletInfo>());
            mock.Setup(m => m.GetAvarableHinbans(It.IsAny<ShippingStationCode>())).Returns(new List<IInventoryPalletInfo>());
            return mock.Object;
        }))();
        return new ReturnInventoryPalletSelector(logger, tempStorageLoader, shippingStorageLoader, shikakariStorageLoader);
    }

    public class SelectReturnInventoryPalletTests
    {
        [Fact]
        public void 空の在庫パレットが存在する場合はそのロケーションを返す() {
            var stationCode = new ShippingStationCode("ST01");
            var location = new LocationCode("SP01");
            var testHinban = new Hinban("FA-1609P(8)BV/U61");
            var emptyPallets = new List<IInventoryPalletInfo> {
                new TestInventoryPalletInfo(location, testHinban, 0)
            };

            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetEmptyPallets(stationCode)).Returns(emptyPallets);
            var service = CreateService(
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );

            var result = service.SelectReturnInventoryPallet(stationCode);

            Assert.Equal(location.Value, result?.Value);
        }
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void 出荷パレット置き場で利用しない在庫パレットのロケーションコードが返される(
            int expected
        ) {
            // 各種コードの生成
            var stationCode = new ShippingStationCode("ST01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var tempLocation1 = new LocationCode("T01");
            var tempLocation2 = new LocationCode("T02");
            var tempStorageItem1 = new TestInventoryPalletInfo(tempLocation1, testHinban1, 10);
            var tempStorageItem2 = new TestInventoryPalletInfo(tempLocation2, testHinban2, 8);
            var tempStorageItems = new List<IInventoryPalletInfo> { tempStorageItem1, tempStorageItem2 };
            var loadableHinban = expected == 1 ? testHinban2 : testHinban1; // exceptedで指定されていない側の品番が積み込み可能

            var shippingMock1 = new Mock<IShippingPalletLoadableHinbanInfo>();
            shippingMock1.Setup(x => x.IsLoadableQuantityGreaterThan(It.IsAny<Hinban>(), 1)).Returns((Hinban hinban, int _) => hinban == loadableHinban);
            var shippingMock2 = new Mock<IShippingPalletLoadableHinbanInfo>();
            shippingMock2.Setup(x => x.IsLoadableQuantityGreaterThan(It.IsAny<Hinban>(), 1)).Returns(false);
            var shippingPalletLoadableInfos = new List<IShippingPalletLoadableHinbanInfo> { shippingMock1.Object, shippingMock2.Object };
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(tempStorageItems);
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shippingPalletLoadableInfos);
            var service = CreateService(
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );
        
            var result = service.SelectReturnInventoryPallet(stationCode);

            var expectedLocation = expected == 1 ? tempLocation1 : tempLocation2;
            Assert.Equal(expectedLocation.Value, result?.Value);
        }
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void 仕掛パレット置き場で利用しない在庫パレットのロケーションコードが返される(
            int expected
        ) {
            // 各種コードの生成
            var stationCode = new ShippingStationCode("ST01");
            var testHinban1 = new Hinban("FA-1609P(8)BV/U61");
            var testHinban2 = new Hinban("FA-1611P(6)BV/Y71");
            var tempLocation1 = new LocationCode("T01");
            var tempLocation2 = new LocationCode("T02");
            var tempStorageItem1 = new TestInventoryPalletInfo(tempLocation1, testHinban1, 10);
            var tempStorageItem2 = new TestInventoryPalletInfo(tempLocation2, testHinban2, 8);
            var tempStorageItems = new List<IInventoryPalletInfo> { tempStorageItem1, tempStorageItem2 };
            var loadableHinban = expected == 1 ? testHinban2 : testHinban1; // exceptedで指定されていない側の品番が積み込み可能

            var shippingMock1 = new Mock<IShippingPalletLoadableHinbanInfo>();
            shippingMock1.Setup(x => x.IsLoadableQuantityGreaterThan(It.IsAny<Hinban>(), 1)).Returns(false);
            var shippingMock2 = new Mock<IShippingPalletLoadableHinbanInfo>();
            shippingMock2.Setup(x => x.IsLoadableQuantityGreaterThan(It.IsAny<Hinban>(), 1)).Returns(false);
            var shippingPalletLoadableInfos = new List<IShippingPalletLoadableHinbanInfo> { shippingMock1.Object, shippingMock2.Object };
            var shikakariMock1 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariMock1.Setup(x => x.IsLoadableQuantityGreaterThan(It.IsAny<Hinban>(), 1)).Returns((Hinban hinban, int _) => hinban == loadableHinban);
            var shikakariMock2 = new Mock<IShikakariPalletLoadableHinbanInfo>();
            shikakariMock2.Setup(x => x.IsLoadableQuantityGreaterThan(It.IsAny<Hinban>(), 1)).Returns(false);
            var shikakariPalletLoadableInfos = new List<IShikakariPalletLoadableHinbanInfo> { shikakariMock1.Object, shikakariMock2.Object };
        
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetAvarableHinbans(stationCode)).Returns(tempStorageItems);
            var shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
            shippingStorageLoaderMock.Setup(x => x.GetLoadableFrom(stationCode, It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shippingPalletLoadableInfos);
            var shikakariStorageLoaderMock = new Mock<IShikakariStorageLoader>();
            shikakariStorageLoaderMock.Setup(x => x.GetLoadableFrom(It.IsAny<IEnumerable<IInventoryPalletInfo>>())).Returns(shikakariPalletLoadableInfos);
            var service = CreateService(
                shippingStorageLoaderParam: shippingStorageLoaderMock.Object,
                shikakariStorageLoaderParam: shikakariStorageLoaderMock.Object,
                tempStorageLoaderParam: tempStorageLoaderMock.Object
            );
        
            var result = service.SelectReturnInventoryPallet(stationCode);

            var expectedLocation = expected == 1 ? tempLocation1 : tempLocation2;
            Assert.Equal(expectedLocation.Value, result?.Value);
        }
    }
}

public record TestInventoryPalletInfo(LocationCode LocationCode, Hinban Hinban, int Quantity): IInventoryPalletInfo;
