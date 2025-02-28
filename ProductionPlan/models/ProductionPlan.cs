using CommonItems.Models;

namespace ProductionPlanManagement.Models;

public class ProductionPlan
{
    public string DeliveryDate { get; set; } = "";
    public string Line { get; set; } = "";
    public string Size { get; set; } = "";
    public int PalletNumber { get; set; }
    public int Priority { get; set; }
    public Hinban Hinban { get; set; } = Hinban.Default;

    public ProductionPlan() { }
    public ProductionPlan(
        string deliveryDate,
        string line,
        string size,
        int palletNumber,
        int priority,
        Hinban hinban
    ) {
        DeliveryDate = deliveryDate;
        Line = line;
        Size = size;
        PalletNumber = palletNumber;
        Priority = priority;
        Hinban = hinban;
    }
}
