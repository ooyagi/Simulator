using CommonItems.Models;

namespace ProductionPlanManagement.Interfaces;

public interface ILoadProductionPlanService
{
    IEnumerable<IProductionPlan> LoadProductionPlans();
}

public interface IProductionPlan
{
    string DeliveryDate { get; }
    string Line { get; }
    string Size { get; }
    string ShippingBay { get; }
    int PalletNumber { get; }
    int Priority { get; }
    Hinban Hinban { get; }
}
