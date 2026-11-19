using AssetTracking2.Data;
using AssetTracking2.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTracking2.Services;

public sealed class AssetService
{
    private readonly AssetDbContext _db;
    public AssetService(AssetDbContext db) => _db = db;

    public List<Asset> All() => _db.Assets.Include(a => a.Office).Include(a => a.Employee)
        .AsEnumerable().OrderBy(a => a is ComputerAsset ? 0 : 1)
        .ThenBy(a => a.PurchaseDate).ToList();

    public Asset? Find(int id) => _db.Assets.Include(a => a.Office).Include(a => a.Employee)
        .FirstOrDefault(a => a.Id == id);

    public void Add(Asset asset) { _db.Assets.Add(asset); _db.SaveChanges(); }
    public void Update() => _db.SaveChanges();
    public bool Delete(int id)
    {
        var asset = _db.Assets.Find(id);
        if (asset is null) return false;
        _db.Assets.Remove(asset);
        _db.SaveChanges();
        return true;
    }

    public List<Asset> Search(string field, string query)
    {
        IQueryable<Asset> assets = _db.Assets.Include(a => a.Office).Include(a => a.Employee);
        var term = query.Trim();
        switch (field)
        {
            case "brand": assets = assets.Where(a => a.Brand.ToLower().Contains(term.ToLower())); break;
            case "model": assets = assets.Where(a => a.ModelName.ToLower().Contains(term.ToLower())); break;
            case "office": assets = assets.Where(a => a.Office.Name.ToLower().Contains(term.ToLower())); break;
            case "year":
                if (!int.TryParse(term, out var year) || year < 1900 || year > 9999) return new();
                assets = assets.Where(a => a.PurchaseDate.Year == year); break;
            default: return new();
        }
        return assets.OrderBy(a => a.PurchaseDate).ToList();
    }

    public List<Asset> Filter(int choice, int officeId = 0)
    {
        var assets = All();
        return choice switch
        {
            1 => assets.Where(a => a.EndOfLife <= DateTime.Today).ToList(),
            2 => assets.Where(a => a is ComputerAsset).ToList(),
            3 => assets.Where(a => a is MobileAsset).ToList(),
            4 => assets.Where(a => a.OfficeId == officeId).ToList(),
            _ => assets
        };
    }
}
