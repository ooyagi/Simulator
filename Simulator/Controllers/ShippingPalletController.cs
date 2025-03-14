using CommonItems.Models;
using Microsoft.AspNetCore.Mvc;

namespace Simulator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShippingPalletController : ControllerBase
{
    private readonly ShippingPalletCoordinator.Interfaces.IInitializationService _shippingPalletCoordinatorInitializationService;
    private readonly ShippingOperationCoordinator.Interfaces.IInitializationService _shippingOperationCoordinatorInitializationService;

    public ShippingPalletController(
        ShippingPalletCoordinator.Interfaces.IInitializationService shippingPalletCoordinatorInitializationService,
        ShippingOperationCoordinator.Interfaces.IInitializationService shippingOperationCoordinatorInitializationService
    ) {
        _shippingPalletCoordinatorInitializationService = shippingPalletCoordinatorInitializationService;
        _shippingOperationCoordinatorInitializationService = shippingOperationCoordinatorInitializationService;
    }

    [HttpPost("SetInitialPallet")]
    public IActionResult Load() {
        _shippingPalletCoordinatorInitializationService.SetInitialShippingPallet();
        return Ok();
    }
    [HttpPost("TakeInitialPallet")] 
    public IActionResult Take() {
        _shippingOperationCoordinatorInitializationService.TakeInitialShippingPallets();
        _shippingOperationCoordinatorInitializationService.TakeInitialInventoryPallets();
        return Ok();
    }
}
