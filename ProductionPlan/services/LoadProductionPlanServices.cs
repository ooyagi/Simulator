using ProductionPlanManagement.Interfaces;
using ProductionPlanManagement.Models;

namespace ProductionPlanManagement.Services;

class LoadProductionPlanService: ILoadProductionPlanService
{
    private readonly IProductionPlanmanagementDbContext _context;
    private readonly ProductionPlanFileReader _reader;

    public LoadProductionPlanService(
        IProductionPlanmanagementDbContext context,
        ProductionPlanFileReader reader
    ) {
        _context = context;
        _reader = reader;
    }

    public void LoadProductionPlans() {
        IEnumerable<ProductionPlanFileRecord> records = _reader.LoadCsvFiles();
        var productionPlans = records.Select(x => ProductionPlanConverter.Convert(x)).ToList();
        _context.ProductionPlans.AddRange(productionPlans);
        _context.SaveChanges();
    }
}
