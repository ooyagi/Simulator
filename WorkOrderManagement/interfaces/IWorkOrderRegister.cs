using CommonItems.Models;

namespace WorkOrderManagement.Interfaces;

public interface IWorkOrderRegister
{
    void Clear();
    void Register(IEnumerable<IProductPlan> plans);
}

public interface IProductPlan
{
    string DeliveryDate { get; }
    string Line { get; }
    string Size { get; }
    int PalletNumber { get; }
    int Priority { get; }
    Hinban Hinban { get; }
}
