using CommonItems.Models;
using InventoryPalletCoordinator.Models;

namespace InventoryPalletCoordinator.Services;

interface IInboundInventoryPalletServices
{
    InventoryPallet Inbound(Hinban hinban);
}
