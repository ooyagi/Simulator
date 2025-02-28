using Microsoft.EntityFrameworkCore;
using ProductionPlanManagement.Interfaces;
using ProductionPlanManagement.Models;

namespace Simulator.Models;

public class DefaultDbContext : DbContext
    , IProductionPlanmanagementDbContext
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    // ProductionPlanManagement
    public DbSet<ProductionPlan> ProductionPlans { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyHinbanConversion();
        modelBuilder.Entity<ProductionPlan>().HasKey(x => new { x.DeliveryDate, x.Line, x.Size, x.PalletNumber, x.Priority });
        modelBuilder.Entity<ProductionPlan>().HasIndex(x => new { x.DeliveryDate, x.Line, x.Size, x.PalletNumber });
    }
}
