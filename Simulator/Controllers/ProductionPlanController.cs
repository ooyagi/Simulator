using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionPlanManagement.Interfaces;

namespace Simulator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductionPlanController : ControllerBase
{
    private readonly ILoadProductionPlanService _loadProductionPlanService;

    public ProductionPlanController(
        ILoadProductionPlanService loadProductionPlanService
    ) {
        _loadProductionPlanService = loadProductionPlanService;
    }

    [HttpPost("load")]
    public IActionResult Load() {
        _loadProductionPlanService.LoadProductionPlans();
        return Ok();
    }
}
