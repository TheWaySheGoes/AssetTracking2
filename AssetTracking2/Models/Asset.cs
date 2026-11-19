namespace AssetTracking2.Models;

public abstract class Asset
{
    public int Id { get; set; }
    public string AssetType { get; set; } = "";
    public string Brand { get; set; } = "";
    public string ModelName { get; set; } = "";
    public DateTime PurchaseDate { get; set; }
    public decimal PurchasePriceUsd { get; set; }
    public string SerialNumber { get; set; } = "";
    public string? Username { get; set; }
    public DateTime WarrantyExpirationDate { get; set; }
    public int OfficeId { get; set; }
    public Office Office { get; set; } = null!;
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? MaintenanceNotes { get; set; }

    protected Asset() { } // Required by Entity Framework
    protected Asset(string type, string brand, string model)
    {
        AssetType = type;
        Brand = brand;
        ModelName = model;
    }

    public DateTime EndOfLife => PurchaseDate.AddYears(3);
    public decimal LocalPrice => Math.Round(PurchasePriceUsd * (Office?.UsdExchangeRate ?? 1m), 2);
    public string Status(DateTime today)
    {
        // The handout has overlapping thresholds. Check <3 months first (YELLOW),
        // then <6 months (RED), as listed. Expired is a separate state.
        if (EndOfLife.Date <= today.Date) return "EXPIRED";
        if (EndOfLife.Date < today.Date.AddMonths(3)) return "YELLOW";
        if (EndOfLife.Date < today.Date.AddMonths(6)) return "RED";
        return "NORMAL";
    }
}

public sealed class ComputerAsset : Asset
{
    public ComputerAsset() { }
    public ComputerAsset(string type, string brand, string model) : base(type, brand, model) { }
}

public sealed class MobileAsset : Asset
{
    public MobileAsset() { }
    public MobileAsset(string type, string brand, string model) : base(type, brand, model) { }
}
