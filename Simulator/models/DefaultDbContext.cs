using Microsoft.EntityFrameworkCore;
using Simulator.Extensions;
using CommonItems.Interfaces;
using CommonItems.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;
using ProductionPlanManagement.Interfaces;
using ProductionPlanManagement.Models;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace Simulator.Models;

public class DefaultDbContext : DbContext
    , ICommonItemsDbContext
    , IProductionPlanmanagementDbContext
    , IInventoryPalletCoordinatorDbContext
    , IShippingPalletCoordinatorDbContext
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

    // IShippingPalletCoordinatorDbContext
    public DbSet<ShippingPallet> ShippingPallets { get; set; }
    public DbSet<ShippingStorage> ShippingStorages { get; set; }
    public DbSet<ShikakariStorage> ShikakariStorages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyHinbanConversion();
        modelBuilder.ApplyTransportTypeConversion();
        modelBuilder.ApplyShippingStationCodeConversion();
        modelBuilder.ApplyLocationCodeConversion();
        modelBuilder.ApplyShippingPalletIDConversion();
        modelBuilder.ApplyInventoryPalletIDConversion();

        modelBuilder.Entity<ProductionPlan>().HasKey(x => new { x.DeliveryDate, x.Line, x.Size, x.PalletNumber, x.Priority });
        modelBuilder.Entity<ProductionPlan>().HasIndex(x => new { x.DeliveryDate, x.Line, x.Size, x.PalletNumber });
    }
}
