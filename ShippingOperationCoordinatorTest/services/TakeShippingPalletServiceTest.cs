using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services.Tests;

public class TakeShippingPalletServiceTest
{
    private static TakeShippingPalletService CreateService(
        IShippingStrageLoader? shippingStrageLoaderParam = null,
        ITakeShippingPalletSelector? takeShippingPalletSelectorParam = null,
        ITakeShippingPalletService? takeShippingPalletServiceParam = null
    ) {
        var logger = NullLogger<TakeShippingPalletService>.Instance;
        var shippingStrageLoader = shippingStrageLoaderParam ?? ((Func<IShippingStrageLoader>)(() => {
            var mock = new Mock<IShippingStrageLoader>();
            mock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(Enumerable.Empty<LocationCode>());
            return mock.Object;
        }))();
        var takeShippingPalletSelector = takeShippingPalletSelectorParam ?? ((Func<ITakeShippingPalletSelector>)(() => {
            var mock = new Mock<ITakeShippingPalletSelector>();
            mock.Setup(x => x.SelectTakeShippingPallet(It.IsAny<ShippingStationCode>())).Returns((ShippingPalletID?)null);
            return mock.Object;
        }))();
        var takeShippingPalletService = takeShippingPalletServiceParam ?? ((Func<ITakeShippingPalletService>)(() => {
            var mock = new Mock<ITakeShippingPalletService>();
            mock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<ShippingPalletID>()));
            return mock.Object;
        }))();
        return new TakeShippingPalletService(
            logger,
            shippingStrageLoader,
            takeShippingPalletSelector,
            takeShippingPalletService
        );
    }

    public class Take
    {
        [Fact]
        public void 空ロケーションが無い場合は取り寄せ依頼をしない() {
            var stationCode = new ShippingStationCode("ST01");
            var shippingStrageLoaderMock = new Mock<IShippingStrageLoader>();
            shippingStrageLoaderMock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(Enumerable.Empty<LocationCode>());
            var takeShippingPalletServiceMock = new Mock<ITakeShippingPalletService>();
            takeShippingPalletServiceMock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<ShippingPalletID>()));
            var service = CreateService(
                shippingStrageLoaderParam: shippingStrageLoaderMock.Object,
                takeShippingPalletServiceParam: takeShippingPalletServiceMock.Object
            );
            
            service.Take(stationCode);
            
            takeShippingPalletServiceMock.Verify(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<ShippingPalletID>()), Times.Never);
        }

        [Fact]
        public void 取り寄せ出荷パレットが無い場合は取り寄せ依頼をしない() {
            var stationCode = new ShippingStationCode("ST01");
            var emptyLocationCode1 = new LocationCode("SP01");
            var emptyLocationCode2 = new LocationCode("SP02");
            var emptyLocations = new List<LocationCode> { emptyLocationCode1, emptyLocationCode2 };

            var shippingStrageLoaderMock = new Mock<IShippingStrageLoader>();
            shippingStrageLoaderMock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(emptyLocations);
            var takeShippingPalletSelectorMock = new Mock<ITakeShippingPalletSelector>();
            takeShippingPalletSelectorMock.Setup(x => x.SelectTakeShippingPallet(It.IsAny<ShippingStationCode>())).Returns((ShippingPalletID?)null);
            var takeShippingPalletServiceMock = new Mock<ITakeShippingPalletService>();
            takeShippingPalletServiceMock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<ShippingPalletID>()));
            var service = CreateService(
                shippingStrageLoaderParam: shippingStrageLoaderMock.Object,
                takeShippingPalletSelectorParam: takeShippingPalletSelectorMock.Object,
                takeShippingPalletServiceParam: takeShippingPalletServiceMock.Object
            );
            
            service.Take(stationCode);
            
            takeShippingPalletServiceMock.Verify(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<ShippingPalletID>()), Times.Never);
        }

        [Theory]
        [InlineData("SP01", "N")]
        [InlineData("SP02", "A")]
        public void 有効なパレットIDが選定された場合は選定されたパレットIDで取り寄せ依頼を行う(
            string location,
            string shippingPalletGroup
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var emptyLocationCode = new LocationCode(location);
            var expectedPalletId = new ShippingPalletID("20250101", shippingPalletGroup, 10);
            var emptyLocations = new List<LocationCode> { emptyLocationCode };

            var shippingStrageLoaderMock = new Mock<IShippingStrageLoader>();
            shippingStrageLoaderMock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(emptyLocations);
            var takeShippingPalletSelectorMock = new Mock<ITakeShippingPalletSelector>();
            takeShippingPalletSelectorMock.Setup(x => x.SelectTakeShippingPallet(It.IsAny<ShippingStationCode>())).Returns(expectedPalletId);
            var takeShippingPalletServiceMock = new Mock<ITakeShippingPalletService>();
            takeShippingPalletServiceMock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<ShippingPalletID>()));
            var service = CreateService(
                shippingStrageLoaderParam: shippingStrageLoaderMock.Object,
                takeShippingPalletSelectorParam: takeShippingPalletSelectorMock.Object,
                takeShippingPalletServiceParam: takeShippingPalletServiceMock.Object
            );
            
            service.Take(stationCode);
            
            takeShippingPalletServiceMock.Verify(x => x.Request(emptyLocationCode, expectedPalletId), Times.Once);
        }
        [Fact]
        public void 空きパレット数より取り寄せ可能なパレット数が少ない場合も選定されたパレットについては取り寄せを行う() {
            var stationCode = new ShippingStationCode("ST01");
            var emptyLocationCode1 = new LocationCode("SP01");
            var emptyLocationCode2 = new LocationCode("SP02");
            var emptyLocations = new List<LocationCode> { emptyLocationCode1, emptyLocationCode2 };
            var expectedPalletId = new ShippingPalletID("20250101", "N", 10);
            int selectCount = 0;

            var shippingStrageLoaderMock = new Mock<IShippingStrageLoader>();
            shippingStrageLoaderMock.Setup(x => x.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(emptyLocations);
            var takeShippingPalletSelectorMock = new Mock<ITakeShippingPalletSelector>();
            takeShippingPalletSelectorMock.Setup(x => x.SelectTakeShippingPallet(It.IsAny<ShippingStationCode>()))
                .Returns(() => {
                    if (selectCount == 0) {
                        selectCount++;
                        return expectedPalletId;
                    } else {
                        return (ShippingPalletID?)null;
                    }
                });
            var takeShippingPalletServiceMock = new Mock<ITakeShippingPalletService>();
            takeShippingPalletServiceMock.Setup(x => x.Request(It.IsAny<LocationCode>(), It.IsAny<ShippingPalletID>()));
            var service = CreateService(
                shippingStrageLoaderParam: shippingStrageLoaderMock.Object,
                takeShippingPalletSelectorParam: takeShippingPalletSelectorMock.Object,
                takeShippingPalletServiceParam: takeShippingPalletServiceMock.Object
            );
            
            service.Take(stationCode);
            
            takeShippingPalletServiceMock.Verify(x => x.Request(It.IsAny<LocationCode>(), expectedPalletId), Times.Once);
        }
    }
}
