using CommonItems.Models;
using Microsoft.AspNetCore.Mvc;
using ProductionPlanManagement.Interfaces;
using WorkOrderManagement.Interfaces;

namespace Simulator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductionPlanController : ControllerBase
{
    private readonly ILoadProductionPlanService _loadProductionPlanService;
    private readonly IWorkOrderRegister _workOrderRegister;

    public ProductionPlanController(
        ILoadProductionPlanService loadProductionPlanService,
        IWorkOrderRegister workOrderRegister
    ) {
        _loadProductionPlanService = loadProductionPlanService;
        _workOrderRegister = workOrderRegister;
    }

    [HttpPost("load")]
    public IActionResult Load() {
        var loadedPlans = _loadProductionPlanService.LoadProductionPlans();
        var paramPlans = loadedPlans.Select(x => new ProductionPlan(x.DeliveryDate, x.Line, x.Size, x.ShippingBay, x.PalletNumber, x.Priority, x.Hinban)).ToList();
        _workOrderRegister.Clear();
        _workOrderRegister.Register(paramPlans);
        return Ok();
    }
    record ProductionPlan(string DeliveryDate, string Line, string Size, string ShippingBay, int PalletNumber, int Priority, Hinban Hinban): WorkOrderManagement.Interfaces.IProductPlan;
}
