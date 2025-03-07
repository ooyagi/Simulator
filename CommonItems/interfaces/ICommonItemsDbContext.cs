using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CommonItems.Models;

namespace CommonItems.Interfaces;

public interface ICommonItemsDbContext
{
    DbSet<TransportRecord> TransportRecords { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    EntityEntry Entry (object entity);
}
