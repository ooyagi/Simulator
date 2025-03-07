using CommonItems.Models;

namespace InventoryPalletCoordinator.Models;

record InventoryPallet(InventoryPalletID Id, Hinban Hinban, int Quantity);
