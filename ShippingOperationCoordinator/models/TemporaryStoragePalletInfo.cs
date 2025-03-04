using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Models;

public record TemporaryStoragePalletInfo(LocationCode LocationCode, Hinban Hinban, int Quantity): IInventoryPalletInfo;
