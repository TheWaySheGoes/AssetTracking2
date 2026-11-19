using AssetTracking2.Models;
using AssetTracking2.Data;

namespace AssetTracking2.Data;

public static class SeedData
{
    public static void AddOffices(AssetDbContext db)
    {
        if (db.Offices.Any()) return;
        // Example FIXED rates. Change these values when needed; they are not live exchange rates.
        db.Offices.AddRange(
            new Office { Name = "Sweden Office", Country = "Sweden", Currency = "SEK", CurrencySymbol = "kr", UsdExchangeRate = 10.0m },
            new Office { Name = "USA Office", Country = "USA", Currency = "USD", CurrencySymbol = "$", UsdExchangeRate = 1m },
            new Office { Name = "Germany Office", Country = "Germany", Currency = "EUR", CurrencySymbol = "€", UsdExchangeRate = 0.92m },
            new Office { Name = "Turkey Office", Country = "Turkey", Currency = "TRY", CurrencySymbol = "₺", UsdExchangeRate = 35m });
        db.SaveChanges();
    }
}
