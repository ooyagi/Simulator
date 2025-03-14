using InventoryPalletCoordinator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InventoryPalletCoordinator.Interfaces;

public interface IInventoryPalletCoordinatorDbContext
{
    public DbSet<StorableHinban> StorableHinbans { get; set; }
    public DbSet<InventoryPallet> InventoryPallets { get; set; }
    public DbSet<InventoryStorage> InventoryStorages { get; set; }
    public DbSet<TemporaryStorage> TemporaryStorages { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    EntityEntry Entry (object entity);
}
