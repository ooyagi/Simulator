using Microsoft.EntityFrameworkCore;
using Simulator.Extensions;
using CommonItems.Interfaces;
using InventoryPalletCoordinator.Interfaces;
using CommonItems.Models;
using ProductionPlanManagement.Interfaces;
using ProductionPlanManagement.Models;
using InventoryPalletCoordinator.Models;

namespace Simulator.Models;

public class DefaultDbContext : DbContext
    , ICommonItemsDbContext
    , IProductionPlanmanagementDbContext
    , IInventoryPalletCoordinatorDbContext
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    // ProductionPlanmanagementDbContext
    public DbSet<ProductionPlan> ProductionPlans { get; set; } = null!;

    // ICommonItemsDbContext
    public DbSet<TransportRecord> TransportRecords { get; set; }

    // IInventoryPalletCoordinatorDbContext
    public DbSet<InventoryPallet> InventoryPallets { get; set; }
    public DbSet<InventoryStorage> InventoryStorages { get; set; }
    public DbSet<TemporaryStorage> TemporaryStorages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyHinbanConversion();
        modelBuilder.ApplyTransportTypeConversion();
        modelBuilder.ApplyLocationCodeConversion();
        modelBuilder.ApplyShippingPalletIDConversion();
        modelBuilder.ApplyInventoryPalletIDConversion();

        modelBuilder.Entity<ProductionPlan>().HasKey(x => new { x.DeliveryDate, x.Line, x.Size, x.PalletNumber, x.Priority });
        modelBuilder.Entity<ProductionPlan>().HasIndex(x => new { x.DeliveryDate, x.Line, x.Size, x.PalletNumber });
    }
}
