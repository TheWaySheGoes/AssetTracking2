using AssetTracking2.Models;

namespace AssetTracking2.Reports;

public static class ReportService
{
    public static void Print(List<Asset> assets, List<Employee> employees)
    {
        Console.WriteLine("\n===== ASSET REPORT =====");
        Console.WriteLine($"Total assets: {assets.Count} | Employees: {employees.Count}");
        foreach (var office in assets.GroupBy(a => a.Office).OrderBy(g => g.Key.Name))
        {
            Console.WriteLine($"{office.Key.Name}: {office.Count()} assets | " +
                $"{office.Sum(a => a.LocalPrice):N2} {office.Key.Currency} (fixed USD rate: {office.Key.UsdExchangeRate})");
        }
        Console.WriteLine("\nClose to end of life (< 6 months, including expired):");
        foreach (var a in assets.Where(a => a.EndOfLife < DateTime.Today.AddMonths(6)).OrderBy(a => a.EndOfLife))
            Console.WriteLine($"{a.Id}: {a.Brand} {a.ModelName} - {a.Status(DateTime.Today)} ({a.EndOfLife:yyyy-MM-dd})");
        Console.WriteLine("\nMost expensive assets (USD):");
        foreach (var a in assets.OrderByDescending(a => a.PurchasePriceUsd).Take(5))
            Console.WriteLine($"{a.Id}: {a.Brand} {a.ModelName} - ${a.PurchasePriceUsd:N2}");
        Console.WriteLine("\nAssets per employee:");
        foreach (var e in employees)
            Console.WriteLine($"{e.FullName}: {assets.Count(a => a.EmployeeId == e.Id)}");
        Console.WriteLine("Most used type: " + (assets.GroupBy(a => a.AssetType)
            .OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "N/A"));
    }
}
