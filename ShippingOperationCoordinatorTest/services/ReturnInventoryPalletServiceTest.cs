using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services.Tests;

public class ReturnInventoryPalletServiceTest
{
    private static ReturnInventoryPalletService CreateService(
        IReturnInventoryPalletSelector? returnInventoryPalletSelectorParam = null,
        IReturnInventoryPalletService? returnInventoryPalletServiceParam = null
    ) {
        var logger = new NullLogger<ReturnInventoryPalletService>();

        var returnInventoryPalletSelector = returnInventoryPalletSelectorParam ?? ((Func<IReturnInventoryPalletSelector>)(() => {
            var mock = new Mock<IReturnInventoryPalletSelector>();
            mock.Setup(m => m.SelectReturnInventoryPallet(It.IsAny<ShippingStationCode>())).Returns((LocationCode?)null);
            return mock.Object;
        }))();
        var returnInventoryPalletService = returnInventoryPalletServiceParam ?? ((Func<IReturnInventoryPalletService>)(() => {
            var mock = new Mock<IReturnInventoryPalletService>();
            mock.Setup(m => m.Request(It.IsAny<LocationCode>()));
            return mock.Object;
        }))();
        var tempStorageLoaderMock = new Mock<ITempStorageLoader>();
        tempStorageLoaderMock.Setup(m => m.GetEmptyLocationCodes(It.IsAny<ShippingStationCode>())).Returns(new List<LocationCode>());
        var tempStorageLoader = tempStorageLoaderMock.Object;
        var takeInventoryPalletServiceMock = new Mock<Services.ITakeInventoryPalletService>();
        takeInventoryPalletServiceMock.Setup(m => m.Take(It.IsAny<ShippingStationCode>()));
        var takeInventoryPalletService = takeInventoryPalletServiceMock.Object;
        return new ReturnInventoryPalletService(logger, tempStorageLoader, returnInventoryPalletSelector, returnInventoryPalletService, takeInventoryPalletService);
    }

    public class Return
    {
        [Fact]
        public void 返却可能パレットが無ければ返却依頼をしない() {
            var stationCode = new ShippingStationCode("ST01");
            var returnInventoryPalletSelector = new Mock<IReturnInventoryPalletSelector>();
            returnInventoryPalletSelector.Setup(m => m.SelectReturnInventoryPallet(It.IsAny<ShippingStationCode>())).Returns((LocationCode?)null);
            var returnInventoryPalletService = new Mock<IReturnInventoryPalletService>();
            var service = CreateService(returnInventoryPalletSelector.Object, returnInventoryPalletService.Object);

            service.Return(stationCode);

            returnInventoryPalletService.Verify(m => m.Request(It.IsAny<LocationCode>()), Times.Never);
        }
        [Theory]
        [InlineData("SP01")]
        [InlineData("SP02")]
        public void 返却可能パレットがあれば返却依頼をする(
            string returnableLocation
        ) {
            var stationCode = new ShippingStationCode("ST01");
            var locationCode = new LocationCode(returnableLocation);
            var returnInventoryPalletSelector = new Mock<IReturnInventoryPalletSelector>();
            returnInventoryPalletSelector.Setup(m => m.SelectReturnInventoryPallet(It.IsAny<ShippingStationCode>())).Returns(locationCode);
            var returnInventoryPalletService = new Mock<IReturnInventoryPalletService>();
            var service = CreateService(returnInventoryPalletSelector.Object, returnInventoryPalletService.Object);

            service.Return(stationCode);

            returnInventoryPalletService.Verify(m => m.Request(locationCode), Times.Once);
        }
    }
}
