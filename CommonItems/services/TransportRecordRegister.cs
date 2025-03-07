using CommonItems.Interfaces;
using CommonItems.Models;

namespace CommonItems.Services;

class TransportRecordRegister: ITransportRecordRegister
{
    private readonly ICommonItemsDbContext _dbContext;

    public TransportRecordRegister(
        ICommonItemsDbContext dbContext
    ) {
        _dbContext = dbContext;
    }

    public void Register(TransportType transportType, LocationCode from, LocationCode to) {
        var transportRecord = new TransportRecord(transportType, from, to);
        _dbContext.TransportRecords.Add(transportRecord);
        _dbContext.SaveChanges();
    }
}
