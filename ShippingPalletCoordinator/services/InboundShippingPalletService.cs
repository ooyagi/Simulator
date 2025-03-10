using Microsoft.Extensions.Logging;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;

namespace ShippingPalletCoordinator.Services;

class InboundShippingPalletService: IInboundShippingPalletService
{
    private readonly ILogger<InboundShippingPalletService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;

    public InboundShippingPalletService(
        ILogger<InboundShippingPalletService> logger,
        IShippingPalletCoordinatorDbContext context
    ) {
        _logger = logger;
        _context = context;
    }

    public void Inbound(ShippingPallet shippingPallet) {
        _logger.LogInformation($"出荷パレットを入庫します: 出荷パレット [{shippingPallet.Id.Value}]");
        _context.ShippingPallets.Add(shippingPallet);
        _context.SaveChanges();
    }
}
