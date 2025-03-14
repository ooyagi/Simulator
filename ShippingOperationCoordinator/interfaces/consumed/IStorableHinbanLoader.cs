using CommonItems.Models;

namespace ShippingOperationCoordinator.Interfaces;

public interface IStorableHinbanLoader
{
    bool IsStorable(Hinban hinban);
}
