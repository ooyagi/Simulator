using CommonItems.Models;
using Microsoft.AspNetCore.Mvc;

namespace Simulator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShippingPalletController : ControllerBase
{
    private readonly ShippingPalletCoordinator.Interfaces.IInitializationService _shippingPalletCoordinatorInitializationService;

    public ShippingPalletController(
        ShippingPalletCoordinator.Interfaces.IInitializationService shippingPalletCoordinatorInitializationService
    ) {
        _shippingPalletCoordinatorInitializationService = shippingPalletCoordinatorInitializationService;
    }

    [HttpPost("SetInitialPallet")]
    public IActionResult Load() {
        _shippingPalletCoordinatorInitializationService.SetInitialShippingPallet();
        return Ok();
    }
    [HttpPost("TakeInitialPallet")] 
    public IActionResult Take() {
        return Ok();
    }
    record ProductionPlan(string DeliveryDate, string Line, string Size, int PalletNumber, int Priority, Hinban Hinban): WorkOrderManagement.Interfaces.IProductPlan;
}
