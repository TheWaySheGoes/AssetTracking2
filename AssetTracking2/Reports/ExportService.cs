using AssetTracking2.Models;
using System.Text;
using System.Text.Json;

namespace AssetTracking2.Reports;

public static class ExportService
{
    public static string Export(List<Asset> assets, int format)
    {
        var dir = Path.Combine(Environment.CurrentDirectory, "Exports");
        Directory.CreateDirectory(dir);
        var rows = assets.Select(a => new
        {
            a.Id,
            a.AssetType,
            a.Brand,
            a.ModelName,
            Office = a.Office.Name,
            PurchaseDate = a.PurchaseDate.ToString("yyyy-MM-dd"),
            a.PurchasePriceUsd,
            LocalPrice = a.LocalPrice,
            Currency = a.Office.Currency,
            Status = a.Status(DateTime.Today)
        }).ToList();
        var path = Path.Combine(dir, $"assets-{DateTime.Now:yyyyMMdd-HHmmss}.{(format == 1 ? "txt" : format == 2 ? "csv" : "json")}");
        if (format == 3)
            File.WriteAllText(path, JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
        else if (format == 2)
        {
            static string Csv(object? v) => "\"" + (v?.ToString() ?? "").Replace("\"", "\"\"") + "\"";
            var sb = new StringBuilder("Id,Type,Brand,Model,Office,PurchaseDate,PriceUsd,LocalPrice,Currency,Status\n");
            foreach (var a in rows)
                sb.AppendLine(string.Join(",", new[] { Csv(a.Id), Csv(a.AssetType), Csv(a.Brand), Csv(a.ModelName),
                    Csv(a.Office), Csv(a.PurchaseDate), Csv(a.PurchasePriceUsd), Csv(a.LocalPrice), Csv(a.Currency), Csv(a.Status) }));
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
        else
        {
            var sb = new StringBuilder("ASSET REPORT\n\n");
            foreach (var a in rows)
                sb.AppendLine($"{a.Id} | {a.AssetType} | {a.Brand} {a.ModelName} | {a.Office} | {a.PurchaseDate} | {a.LocalPrice:N2} {a.Currency} | {a.Status}");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
        return path;
    }
}
