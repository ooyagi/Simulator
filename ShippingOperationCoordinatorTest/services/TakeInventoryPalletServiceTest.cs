using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services.Tests;

public class TakeInventoryPalletServiceTest
{
    private static TakeInventoryPalletService CreateService(
        ITempStorageLoader? tempStorageLoaderParam = null,
        ITakeInventoryPalletSelector? takeInventoryPalletSelectorParam = null,
        ITakeInventoryPalletService? takeInventoryPalletServiceParam = null
    ) {
        var logger = NullLogger<TakeInventoryPalletService>.Instance;
        var tempStorageLoader = tempStorageLoaderParam ?? ((Func<ITempStorageLoader>)(() => {
            var mock = new Mock<ITempStorageLoader>();
            mock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(Enumerable.Empty<LocationCode>());
            return mock.Object;
        }))();
        var takeInventoryPalletSelector = takeInventoryPalletSelectorParam ?? ((Func<ITakeInventoryPalletSelector>)(() => {
            var mock = new Mock<ITakeInventoryPalletSelector>();
            mock.Setup(x => x.SelectTakeInventoryPallet(It.IsAny<ShippingStationCode>())).Returns((Hinban?)null);
            return mock.Object;
        }))();
        var takeInventoryPalletService = takeInventoryPalletServiceParam ?? ((Func<ITakeInventoryPalletService>)(() => {
            var mock = new Mock<ITakeInventoryPalletService>();
            mock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<Hinban>()));
            return mock.Object;
        }))();
        return new TakeInventoryPalletService(
            logger,
            tempStorageLoader,
            takeInventoryPalletSelector,
            takeInventoryPalletService
        );
    }

    public class Take
    {
        [Fact]
        public void 空ロケーションが無い場合は取り寄せ依頼をしない() {
            var stationCode = new ShippingStationCode("ST01");
            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(Enumerable.Empty<LocationCode>());
            var takeInventoryPalletServiceMock = new Mock<ITakeInventoryPalletService>();
            takeInventoryPalletServiceMock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<Hinban>()));
            var service = CreateService(
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                takeInventoryPalletServiceParam: takeInventoryPalletServiceMock.Object
            );
            
            service.Take(stationCode);
            
            takeInventoryPalletServiceMock.Verify(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<Hinban>()), Times.Never);
        }
        [Fact]
        public void 取り寄せ出荷パレットが無い場合は取り寄せ依頼をしない() {
            var stationCode = new ShippingStationCode("ST01");
            var emptyLocationCode1 = new LocationCode("SP01");
            var emptyLocationCode2 = new LocationCode("SP02");
            var emptyLocations = new List<LocationCode> { emptyLocationCode1, emptyLocationCode2 };

            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(emptyLocations);
            var takeInventoryPalletSelectorMock = new Mock<ITakeInventoryPalletSelector>();
            takeInventoryPalletSelectorMock.Setup(x => x.SelectTakeInventoryPallet(It.IsAny<ShippingStationCode>())).Returns((Hinban?)null);
            var takeInventoryPalletServiceMock = new Mock<ITakeInventoryPalletService>();
            takeInventoryPalletServiceMock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<Hinban>()));
            var service = CreateService(
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                takeInventoryPalletSelectorParam: takeInventoryPalletSelectorMock.Object,
                takeInventoryPalletServiceParam: takeInventoryPalletServiceMock.Object
            );
            
            service.Take(stationCode);
            
            takeInventoryPalletServiceMock.Verify(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<Hinban>()), Times.Never);
        }

        [Theory]
        [InlineData("FA-1609P(8)BV/U61")]
        [InlineData("FA-1611P(6)BV/Y71")]
        public void 有効な品番が選定された場合は選定された品番で取り寄せ依頼を行う(
            string selectedHinban
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var testLocationCode = new LocationCode("SP01");
            var testLocations = new List<LocationCode> { testLocationCode };
            var hinban = new Hinban(selectedHinban);

            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(testLocations);
            var takeInventoryPalletSelectorMock = new Mock<ITakeInventoryPalletSelector>();
            takeInventoryPalletSelectorMock.Setup(x => x.SelectTakeInventoryPallet(It.IsAny<ShippingStationCode>())).Returns(hinban);
            var takeInventoryPalletServiceMock = new Mock<ITakeInventoryPalletService>();
            takeInventoryPalletServiceMock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<Hinban>()));
            var service = CreateService(
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                takeInventoryPalletSelectorParam: takeInventoryPalletSelectorMock.Object,
                takeInventoryPalletServiceParam: takeInventoryPalletServiceMock.Object
            );
            
            service.Take(stationCode);
            
            takeInventoryPalletServiceMock.Verify(x => x.Request(testLocationCode, hinban), Times.Once);
        }
        [Fact]
        public void 空きパレット数より取り寄せ可能なパレット数が少ない場合も選定されたパレットについては取り寄せを行う() {
            var stationCode = new ShippingStationCode("ST01");
            var emptyLocationCode1 = new LocationCode("SP01");
            var emptyLocationCode2 = new LocationCode("SP02");
            var emptyLocations = new List<LocationCode> { emptyLocationCode1, emptyLocationCode2 };
            var exeptedHinban = new Hinban("FA-1609P(8)BV/U61");
            int selectCount = 0;

            var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
            tempStorageLoaderMock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(emptyLocations);
            var takeInventoryPalletSelectorMock = new Mock<ITakeInventoryPalletSelector>();
            takeInventoryPalletSelectorMock.Setup(x => x.SelectTakeInventoryPallet(It.IsAny<ShippingStationCode>()))
                .Returns(() => {
                    if (selectCount == 0) {
                        selectCount++;
                        return exeptedHinban;
                    } else {
                        return (Hinban?)null;
                    }
                });
            var takeInventoryPalletServiceMock = new Mock<ITakeInventoryPalletService>();
            takeInventoryPalletServiceMock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<Hinban>()));
            var service = CreateService(
                tempStorageLoaderParam: tempStorageLoaderMock.Object,
                takeInventoryPalletSelectorParam: takeInventoryPalletSelectorMock.Object,
                takeInventoryPalletServiceParam: takeInventoryPalletServiceMock.Object
            );
            
            service.Take(stationCode);
            
            takeInventoryPalletServiceMock.Verify(x => x.Request(It.IsAny<LocationCode>(), exeptedHinban), Times.Once);
        }
    }
}
