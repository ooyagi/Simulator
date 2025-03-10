using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShippingOperationCoordinator.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IShippingOperationCoordinatorDbContext
{
    DbSet<ShippingStation> ShippingStations { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    EntityEntry Entry (object entity);
}
