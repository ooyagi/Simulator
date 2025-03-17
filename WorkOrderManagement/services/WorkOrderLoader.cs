using Microsoft.Extensions.Logging;
using CommonItems.Models;
using WorkOrderManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace WorkOrderManagement.Services;

public class WorkOrderLoader: ShippingOperationCoordinator.Interfaces.IWorkOrderLoader, ShippingPalletCoordinator.Interfaces.IWorkOrderLoader
{
    private readonly ILogger<WorkOrderLoader> _logger;
    private readonly IWorkOrderDbContext _context;

    public WorkOrderLoader(
        ILogger<WorkOrderLoader> logger,
        IWorkOrderDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public ShippingPalletCoordinator.Interfaces.IWorkOrder? GetNextOrder() {
        var order = _context.WorkOrders.Include(x => x.OrderedItems)
            .Where(x => x.Assigned == false)
            .OrderBy(x => x.Priority)
            .FirstOrDefault();
        if (order == null) {
            _logger.LogInformation("次の作業指示が見つかりませんでした");
            return null;
        }
        order.Assigned = true;
        _context.SaveChanges();
        return new WorkOrder(order.PalletID, order.Priority, order.OrderedItems.Select(x => new WorkItem(x.Hinban, x.Index)));
    }
    public IEnumerable<ShippingPalletID> GetUsageWorkOrderIdByHinban(Hinban hinban) {
        return _context.WorkOrders
            .Where(x => x.Assigned == false && x.OrderedItems.Any(x => x.Hinban == hinban))
            .Select(x => x.PalletID)
            .ToList();
    }
    record WorkOrder(ShippingPalletID ShippingPalletID, int Priority, IEnumerable<ShippingPalletCoordinator.Interfaces.IWorkItem> WorkItems): ShippingPalletCoordinator.Interfaces.IWorkOrder;
    record WorkItem(Hinban Hinban, int Index): ShippingPalletCoordinator.Interfaces.IWorkItem;
}
