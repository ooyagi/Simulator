using CommonItems.Models;

namespace CommonItems.Interfaces;

public interface ITransportRecordRegister
{
    void Register(TransportType transportType, LocationCode from, LocationCode to);
    void Clear();
}
