using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WorkOrderManagement.Models;

namespace WorkOrderManagement.Interfaces;

public interface IWorkOrderDbContext
{
    DbSet<WorkOrder> WorkOrders { get; set; }
    DbSet<OrderedItem> OrderedItems { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    EntityEntry Entry (object entity);
}
