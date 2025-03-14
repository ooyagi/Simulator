using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services.Tests;

public class ReturnShippingPalletServiceTest
{
    private static ReturnShippingPalletService CreateService(
        IReturnShippingPalletSelector? returnShippingPalletSelectorParam = null,
        IReturnShippingPalletService? returnShippingPalletServiceParam = null
    ) {
        var logger = new NullLogger<ReturnShippingPalletService>();

        var returnShippingPalletSelector = returnShippingPalletSelectorParam ?? ((Func<IReturnShippingPalletSelector>)(() => {
            var mock = new Mock<IReturnShippingPalletSelector>();
            mock.Setup(m => m.SelectReturnShippingPallet(It.IsAny<ShippingStationCode>())).Returns((LocationCode?)null);
            return mock.Object;
        }))();
        var returnShippingPalletService = returnShippingPalletServiceParam ?? ((Func<IReturnShippingPalletService>)(() => {
            var mock = new Mock<IReturnShippingPalletService>();
            mock.Setup(m => m.Request(It.IsAny<LocationCode>()));
            return mock.Object;
        }))();
        var _shippingStorageLoaderMock = new Mock<IShippingStorageLoader>();
        _shippingStorageLoaderMock.Setup(m => m.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(new List<LocationCode>());
        var shippingStorageLoader = _shippingStorageLoaderMock.Object;
        var takeShippingPalletSelectorMock = new Mock<ITakeShippingPalletSelector>();
        takeShippingPalletSelectorMock.Setup(m => m.CheckEnableShippingPalletInShikakariStorage(It.IsAny<ShippingStationCode>())).Returns(true);
        var takeShippingPalletSelector = takeShippingPalletSelectorMock.Object;
        var takeShippingPalletServiceMock = new Mock<Services.ITakeShippingPalletService>();
        takeShippingPalletServiceMock.Setup(m => m.Take(It.IsAny<ShippingStationCode>()));
        var takeShippingPalletService = takeShippingPalletServiceMock.Object;
        return new ReturnShippingPalletService(logger, shippingStorageLoader, returnShippingPalletSelector, takeShippingPalletSelector, returnShippingPalletService, takeShippingPalletService);
    }

    public class Return
    {
        [Fact]
        public void 返却可能パレットが無ければ返却依頼をしない() {
            var stationCode = new ShippingStationCode("ST01");
            var returnShippingPalletSelector = new Mock<IReturnShippingPalletSelector>();
            returnShippingPalletSelector.Setup(m => m.SelectReturnShippingPallet(It.IsAny<ShippingStationCode>())).Returns((LocationCode?)null);
            var returnShippingPalletService = new Mock<IReturnShippingPalletService>();
            var service = CreateService(returnShippingPalletSelector.Object, returnShippingPalletService.Object);

            service.Return(stationCode);

            returnShippingPalletService.Verify(m => m.Request(It.IsAny<LocationCode>()), Times.Never);
        }
        [Theory]
        [InlineData("SP01")]
        [InlineData("SP02")]
        public void 返却可能パレットがあれば返却依頼をする(
            string returnableLocation
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var locationCode = new LocationCode(returnableLocation);
            var returnShippingPalletSelector = new Mock<IReturnShippingPalletSelector>();
            returnShippingPalletSelector.Setup(m => m.SelectReturnShippingPallet(It.IsAny<ShippingStationCode>())).Returns(locationCode);
            var returnShippingPalletService = new Mock<IReturnShippingPalletService>();
            var service = CreateService(returnShippingPalletSelector.Object, returnShippingPalletService.Object);

            service.Return(stationCode);

            returnShippingPalletService.Verify(m => m.Request(locationCode), Times.Once);
        }
    }
}
