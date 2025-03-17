using Microsoft.EntityFrameworkCore;
using Simulator.Extensions;
using CommonItems.Interfaces;
using CommonItems.Models;
using WorkOrderManagement.Interfaces;
using WorkOrderManagement.Models;
using InventoryPalletCoordinator.Interfaces;
using InventoryPalletCoordinator.Models;
using ShippingOperationCoordinator.Interfaces;
using ShippingOperationCoordinator.Models;
using ProductionPlanManagement.Interfaces;
using ProductionPlanManagement.Models;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace Simulator.Models;

public class DefaultDbContext : DbContext
    , ICommonItemsDbContext
    , IWorkOrderDbContext
    , IProductionPlanmanagementDbContext
    , IShippingOperationCoordinatorDbContext
    , IInventoryPalletCoordinatorDbContext
    , IShippingPalletCoordinatorDbContext
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    // ICommonItemsDbContext
    public DbSet<TransportRecord> TransportRecords { get; set; }

    // WorkOrderDbContext
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<OrderedItem> OrderedItems { get; set; }

    // ProductionPlanmanagementDbContext
    public DbSet<ProductionPlan> ProductionPlans { get; set; }

    // IShippingOperationCoordinatorDbContext
    public DbSet<ShippingStation> ShippingStations { get; set; }

    // IInventoryPalletCoordinatorDbContext
    public DbSet<StorableHinban> StorableHinbans { get; set; }
    public DbSet<InventoryPallet> InventoryPallets { get; set; }
    public DbSet<InventoryStorage> InventoryStorages { get; set; }
    public DbSet<TemporaryStorage> TemporaryStorages { get; set; }

    // IShippingPalletCoordinatorDbContext
    public DbSet<ShippingPallet> ShippingPallets { get; set; }
    public DbSet<ShippingPalletItem> ShippingPalletItems { get; set; }
    public DbSet<ShippingStorage> ShippingStorages { get; set; }
    public DbSet<ShikakariStorage> ShikakariStorages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyHinbanConversion();
        modelBuilder.ApplyTransportTypeConversion();
        modelBuilder.ApplyShippingStationCodeConversion();
        modelBuilder.ApplyLocationCodeConversion();
        modelBuilder.ApplyShippingPalletIDConversion();
        modelBuilder.ApplyInventoryPalletIDConversion();

        modelBuilder.Entity<ProductionPlan>().HasKey(x => new { x.DeliveryDate, x.Line, x.Size, x.ShippingBay, x.PalletNumber, x.Priority });
        modelBuilder.Entity<ProductionPlan>().HasIndex(x => new { x.DeliveryDate, x.Line, x.Size, x.PalletNumber });
        modelBuilder.Entity<WorkOrder>().HasMany(x => x.OrderedItems).WithOne().HasForeignKey(x => x.PalletID);
        modelBuilder.Entity<OrderedItem>().HasKey(x => new { x.PalletID, x.Index });
        modelBuilder.Entity<ShippingPallet>().HasMany(x => x.Items).WithOne().HasForeignKey(x => x.PalletID);
        modelBuilder.Entity<ShippingPalletItem>().HasKey(x => new { x.PalletID, x.Index });
    }
}
