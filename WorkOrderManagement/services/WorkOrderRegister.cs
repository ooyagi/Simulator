using CommonItems.Models;
using Microsoft.Extensions.Logging;
using WorkOrderManagement.Interfaces;
using WorkOrderManagement.Models;

namespace WorkOrderManagement.Services;

class WorkOrderRegister: IWorkOrderRegister
{
    private readonly ILogger<WorkOrderRegister> _logger;
    private readonly IWorkOrderDbContext _context;

    public WorkOrderRegister(
        ILogger<WorkOrderRegister> logger,
        IWorkOrderDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public void Register(IEnumerable<IProductPlan> plans) {
        _logger.LogInformation("作業指示を登録します");
        try {
            var orders = plans
                .GroupBy(x => new {x.DeliveryDate, x.Line, x.Size, x.PalletNumber})
                .Select(x => {
                    var id = new ShippingPalletID(x.Key.DeliveryDate, (x.Key.Line + x.Key.Size), x.Key.PalletNumber);
                    var items = x.Select(x => new OrderedItem(id, x.Hinban, x.Priority));
                    return new WorkOrder(
                        id,
                        x.Key.DeliveryDate,
                        x.Key.Line,
                        x.Key.Size,
                        x.Key.PalletNumber,
                        items
                    );
                }).ToList();
            _context.WorkOrders.AddRange(orders);
            _context.SaveChanges();
        } catch (Exception e) {
            _logger.LogError(e, "作業指示の登録に失敗しました");
            throw;
        }
    }
}
