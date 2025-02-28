using LINQtoCSV;

namespace ProductionPlanManagement.Models;

class ProductionPlanFileRecord
{
    [CsvColumn(FieldIndex = 1)]
    public string DeliveryDate { get; set; } = "";
    [CsvColumn(FieldIndex = 2)]
    public string Line { get; set; } = "";
    [CsvColumn(FieldIndex = 3)]
    public string Size { get; set; } = "";
    [CsvColumn(FieldIndex = 4)]
    public string ShippingBay { get; set; } = "";
    [CsvColumn(FieldIndex = 5)]
    public int Rank { get; set; } = 0;
    [CsvColumn(FieldIndex = 6)]
    public string SerialNumber { get; set; } = "";
    [CsvColumn(FieldIndex = 7)]
    public string Hinban { get; set; } = "";
    [CsvColumn(FieldIndex = 8)]
    public string Bay { get; set; } = "";
    [CsvColumn(FieldIndex = 9)]
    public int PalletNumber { get; set; }
    [CsvColumn(FieldIndex = 10)]
    public int Priority { get; set; }
}
