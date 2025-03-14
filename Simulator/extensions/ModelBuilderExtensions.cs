using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CommonItems.Models;
using ProductionPlanManagement.Models;
using WorkOrderManagement.Models;
using ShippingOperationCoordinator.Models;
using InventoryPalletCoordinator.Models;
using ShippingPalletCoordinator.Models;

namespace Simulator.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyHinbanConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<Hinban, string>(v => v.Value, v => new Hinban(v));

        // WorkOrderManagement
        modelBuilder.Entity<OrderedItem>().Property(typeof(Hinban), "Hinban").HasConversion(conversion);

        // ProductionPlanManagement
        modelBuilder.Entity<ProductionPlan>().Property(typeof(Hinban), "Hinban").HasConversion(conversion);

        // InventoryPalletCoordinator
        modelBuilder.Entity<StorableHinban>().Property(typeof(Hinban), "Hinban").HasConversion(conversion);
        modelBuilder.Entity<InventoryPallet>().Property(typeof(Hinban), "Hinban").HasConversion(conversion);

        // ShippingPalletCoordinator
        modelBuilder.Entity<ShippingPalletItem>().Property(typeof(Hinban), "Hinban").HasConversion(conversion);
    }
    public static void ApplyTransportTypeConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<TransportType, string>(v => v.Value, v => new TransportType(v));

        // CommonItems
        modelBuilder.Entity<TransportRecord>().Property(typeof(TransportType), "Type").HasConversion(conversion);
    }
    public static void ApplyShippingStationCodeConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<ShippingStationCode, string>(v => v.Value, v => new ShippingStationCode(v));

        // ShippingOperationCoordinator
        modelBuilder.Entity<ShippingStation>().Property(typeof(ShippingStationCode), "Code").HasConversion(conversion);

        // InventoryPalletCoordinator
        modelBuilder.Entity<TemporaryStorage>().Property(typeof(ShippingStationCode), "ShippingStationCode").HasConversion(conversion);
        // ShippingPalletCoordinator
        modelBuilder.Entity<ShippingStorage>().Property(typeof(ShippingStationCode), "ShippingStationCode").HasConversion(conversion);
    }
    public static void ApplyLocationCodeConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<LocationCode, string>(v => v.Value, v => new LocationCode(v));

        // CommonItems
        modelBuilder.Entity<TransportRecord>().Property(typeof(LocationCode), "From").HasConversion(conversion);
        modelBuilder.Entity<TransportRecord>().Property(typeof(LocationCode), "To").HasConversion(conversion);

        // InventoryPalletCoordinator
        modelBuilder.Entity<InventoryStorage>().Property(typeof(LocationCode), "LocationCode").HasConversion(conversion);
        modelBuilder.Entity<TemporaryStorage>().Property(typeof(LocationCode), "LocationCode").HasConversion(conversion);

        // ShippingPalletCoordinator
        modelBuilder.Entity<ShippingStorage>().Property(typeof(LocationCode), "LocationCode").HasConversion(conversion);
        modelBuilder.Entity<ShikakariStorage>().Property(typeof(LocationCode), "LocationCode").HasConversion(conversion);
    }
    public static void ApplyShippingPalletIDConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<ShippingPalletID, string>(v => v.Value, v => new ShippingPalletID(v));

        // WorkOrderManagement
        modelBuilder.Entity<WorkOrder>().Property(typeof(ShippingPalletID), "PalletID").HasConversion(conversion);
        modelBuilder.Entity<OrderedItem>().Property(typeof(ShippingPalletID), "PalletID").HasConversion(conversion);

        // ShippingPalletCoordinator
        modelBuilder.Entity<ShippingPallet>().Property(typeof(ShippingPalletID), "Id").HasConversion(conversion);
        modelBuilder.Entity<ShippingPalletItem>().Property(typeof(ShippingPalletID), "PalletID").HasConversion(conversion);
        modelBuilder.Entity<ShippingStorage>().Property(typeof(ShippingPalletID), "ShippingPalletID").HasConversion(conversion);
        modelBuilder.Entity<ShikakariStorage>().Property(typeof(ShippingPalletID), "ShippingPalletID").HasConversion(conversion);
    }
    public static void ApplyInventoryPalletIDConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<InventoryPalletID, string>(v => v.Value, v => new InventoryPalletID(v));

        // InventoryPalletCoordinator
        modelBuilder.Entity<InventoryPallet>().Property(typeof(InventoryPalletID), "Id").HasConversion(conversion);
        modelBuilder.Entity<InventoryStorage>().Property(typeof(InventoryPalletID), "InventoryPalletID").HasConversion(conversion);
        modelBuilder.Entity<TemporaryStorage>().Property(typeof(InventoryPalletID), "InventoryPalletID").HasConversion(conversion);
    }
}
