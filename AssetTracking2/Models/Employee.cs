using AssetTracking2.Models;

namespace AssetTracking2.Models;

public sealed class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Department { get; set; } = "";
    public string Email { get; set; } = "";
    public List<Asset> AssignedAssets { get; set; } = new();
}
