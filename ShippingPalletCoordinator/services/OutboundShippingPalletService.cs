using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingPalletCoordinator.Interfaces;

namespace ShippingPalletCoordinator.Services;

class OutboundShippingPalletService: IOutboundShippingPalletService
{
    private readonly ILogger<OutboundShippingPalletService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;
    private readonly IShippingPalletLoader _shippingPalletLoader;

    public OutboundShippingPalletService(
        ILogger<OutboundShippingPalletService> logger,
        IShippingPalletCoordinatorDbContext context,
        IShippingPalletLoader shippingPalletLoader
    ) {
        _logger = logger;
        _context = context;
        _shippingPalletLoader = shippingPalletLoader;
    }

    public void Outbound(ShippingPalletID shippingPalletId) {
        _logger.LogInformation($"出荷パレットを出庫します: 出荷パレット [{shippingPalletId.Value}]");
        var pallet = _shippingPalletLoader.Find(shippingPalletId);
        if (pallet == null) {
            return;
        }
        _context.ShippingPallets.Remove(pallet);
        _context.SaveChanges();
    }
}
