using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Interfaces;

public interface IShippingPalletCoordinatorDbContext
{
    public DbSet<ShippingPallet> ShippingPallets { get; set; }
    public DbSet<ShippingPalletItem> ShippingPalletItems { get; set; }
    public DbSet<ShippingStorage> ShippingStorages { get; set; }
    public DbSet<ShikakariStorage> ShikakariStorages { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    EntityEntry Entry (object entity);
}
