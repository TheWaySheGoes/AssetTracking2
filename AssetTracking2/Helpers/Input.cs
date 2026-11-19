using System.Globalization;

namespace AssetTracking2.Helpers;

public static class Input
{
    public static string Required(string label, string? existing = null)
    {
        while (true)
        {
            Console.Write($"{label}{(existing is null ? "" : $" [{existing}]")}: ");
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(value)) return value;
            if (existing is not null) return existing;
            Console.WriteLine("Value cannot be empty.");
        }
    }

    public static string? Optional(string label, string? existing = null)
    {
        Console.Write($"{label}{(existing is null ? "" : $" [{existing}]")} (blank to keep/skip): ");
        var value = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(value) ? existing : value;
    }

    public static int Number(string label, int min, int max)
    {
        while (true)
        {
            Console.Write($"{label} ({min}-{max}): ");
            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max) return value;
            Console.WriteLine("Invalid number.");
        }
    }

    public static DateTime Date(string label, DateTime? current = null)
    {
        while (true)
        {
            Console.Write($"{label} (yyyy-MM-dd){(current is null ? "" : $" [{current:yyyy-MM-dd}]")}: ");
            var raw = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(raw) && current.HasValue) return current.Value;
            if (DateTime.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date) && date.Year >= 1900 && date <= DateTime.Today.AddYears(10)) return date;
            Console.WriteLine("Use a valid yyyy-MM-dd date.");
        }
    }

    public static decimal Money(string label, decimal? current = null)
    {
        while (true)
        {
            Console.Write($"{label} (USD){(current is null ? "" : $" [{current:0.00}]")}: ");
            var raw = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(raw) && current.HasValue) return current.Value;
            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) && price >= 0) return price;
            Console.WriteLine("Enter a non-negative amount (use . for decimals).");
        }
    }
}
