using CommonItems.Models;

namespace ProductionPlanManagement.Models;

class ProductionPlanConverter
{
    public static ProductionPlan Convert(ProductionPlanFileRecord record) {
        return new ProductionPlan(
            record.DeliveryDate,
            record.Line,
            record.Size,
            record.PalletNumber,
            record.Priority,
            new Hinban(record.Hinban)
        );
    }
}
