using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CommonItems.Models;
using ProductionPlanManagement.Models;
using InventoryPalletCoordinator.Models;

namespace Simulator.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyHinbanConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<Hinban, string>(v => v.Value, v => new Hinban(v));

        // ProductionPlanManagement
        modelBuilder.Entity<ProductionPlan>().Property(typeof(Hinban), "Hinban").HasConversion(conversion);

        // InventoryPalletCoordinator
        modelBuilder.Entity<InventoryPallet>().Property(typeof(Hinban), "Hinban").HasConversion(conversion);
    }
    public static void ApplyTransportTypeConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<TransportType, string>(v => v.Value, v => new TransportType(v));

        // CommonItems
        modelBuilder.Entity<TransportRecord>().Property(typeof(TransportType), "Type").HasConversion(conversion);
    }
    public static void ApplyLocationCodeConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<LocationCode, string>(v => v.Value, v => new LocationCode(v));

        // CommonItems
        modelBuilder.Entity<TransportRecord>().Property(typeof(LocationCode), "From").HasConversion(conversion);
        modelBuilder.Entity<TransportRecord>().Property(typeof(LocationCode), "To").HasConversion(conversion);

        // InventoryPalletCoordinator
        modelBuilder.Entity<InventoryStorage>().Property(typeof(LocationCode), "LocationCode").HasConversion(conversion);
        modelBuilder.Entity<TemporaryStorage>().Property(typeof(LocationCode), "LocationCode").HasConversion(conversion);
    }
    public static void ApplyShippingPalletIDConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<ShippingPalletID, string>(v => v.Value, v => new ShippingPalletID(v));
    }
    public static void ApplyInventoryPalletIDConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<InventoryPalletID, string>(v => v.Value, v => new InventoryPalletID(v));

        // InventoryPalletCoordinator
        modelBuilder.Entity<InventoryPallet>().Property(typeof(InventoryPalletID), "InventoryPalletID").HasConversion(conversion);
        modelBuilder.Entity<InventoryStorage>().Property(typeof(InventoryPalletID), "InventoryPalletID").HasConversion(conversion);
        modelBuilder.Entity<TemporaryStorage>().Property(typeof(InventoryPalletID), "InventoryPalletID").HasConversion(conversion);
    }
}
