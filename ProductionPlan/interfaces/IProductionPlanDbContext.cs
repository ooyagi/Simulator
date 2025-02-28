using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProductionPlanManagement.Models;

namespace ProductionPlanManagement.Interfaces;

public interface IProductionPlanmanagementDbContext
{
    public DbSet<ProductionPlan> ProductionPlans { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    EntityEntry Entry (object entity);
}
