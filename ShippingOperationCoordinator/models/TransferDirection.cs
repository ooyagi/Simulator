using CommonItems.Models;

namespace ShippingOperationCoordinator.Models;
public record TransferDirection(Hinban Hinban, LocationCode From, LocationCode To);
