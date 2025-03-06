using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface ITransferService
{
    bool ExecuteTransfer(ShippingStationCode stationCode);
}
