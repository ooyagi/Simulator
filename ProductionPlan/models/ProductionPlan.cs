using CommonItems.Models;
using ProductionPlanManagement.Interfaces;

namespace ProductionPlanManagement.Models;

public class ProductionPlan: IProductionPlan
{
    public string DeliveryDate { get; set; } = "";
    public string Line { get; set; } = "";
    public string Size { get; set; } = "";
    public string ShippingBay { get; set; } = "";
    public int PalletNumber { get; set; }
    public int Priority { get; set; }
    public Hinban Hinban { get; set; } = Hinban.Default;

    public ProductionPlan() { }
    public ProductionPlan(
        string deliveryDate,
        string line,
        string size,
        string shippingBay,
        int palletNumber,
        int priority,
        Hinban hinban
    ) {
        DeliveryDate = deliveryDate;
        Line = line;
        Size = size;
        ShippingBay = shippingBay;
        PalletNumber = palletNumber;
        Priority = priority;
        Hinban = hinban;
    }
}
