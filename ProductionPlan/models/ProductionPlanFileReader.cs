using System.Text.RegularExpressions;
using LINQtoCSV;
using Microsoft.Extensions.Options;

namespace ProductionPlanManagement.Models;

class ProductionPlanFileReader
{
    private readonly string _folderPath;

    public ProductionPlanFileReader(
        IOptions<ProductionPlanConfig> config   
    ) {
        _folderPath = config.Value.ProductionPlanFilePath;
    }

    public IEnumerable<ProductionPlanFileRecord> LoadCsvFiles() {
        var csvContext = new CsvContext();
        var csvFileDescription = new CsvFileDescription {
            SeparatorChar = ',',
            EnforceCsvColumnAttribute = true,
            FirstLineHasColumnNames = false,
        };
        var file = GetFileName(_folderPath);
        if (file == null) {
            Console.WriteLine($"No file found in {_folderPath}");
            return new List<ProductionPlanFileRecord>();
        }
        try {
            return csvContext.Read<ProductionPlanFileRecord>(file, csvFileDescription).ToList();
        } catch (Exception ex) {
            Console.WriteLine($"Error reading file {file}: {ex.Message}");
            throw;
        }
    }
    private string? GetFileName(string folderPath) {
        return Directory.GetFiles(folderPath, "*.csv")
            .Where(file => Regex.IsMatch(Path.GetFileName(file), @"[A-Z][A-Z]_\d{8}_\d{6}\.csv"))
            .FirstOrDefault();
    }
}
