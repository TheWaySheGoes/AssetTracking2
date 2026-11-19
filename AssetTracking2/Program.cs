using AssetTracking2.Data;
using AssetTracking2.Helpers;
using AssetTracking2.Models;
using AssetTracking2.Reports;
using AssetTracking2.Services;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = System.Text.Encoding.UTF8;
using var db = new AssetDbContext();
try
{
    if (!db.Database.GetMigrations().Any())
    {
        Console.WriteLine("No EF Core migration found. Follow README: dotnet ef migrations add InitialCreate, then dotnet ef database update.");
        return;
    }
    db.Database.Migrate();
    SeedData.AddOffices(db);
}
catch (Exception ex)
{
    Console.WriteLine("Database setup failed: " + ex.Message);
    Console.WriteLine("Run the migration commands from README.md.");
    return;
}

var service = new AssetService(db);
Console.WriteLine("SMART ASSET TRACKING SYSTEM");
Console.WriteLine("Demo login (simulated roles; not password authentication)");
var role = Input.Number("1 Admin | 2 Manager | 3 Employee", 1, 3);
int? loggedEmployee = null;
if (role == 3)
{
    var people = db.Employees.OrderBy(e => e.FullName).ToList();
    if (people.Count == 0)
        Console.WriteLine("No employees registered yet. Ask an Admin to add employees.");
    else
    {
        foreach (var e in people) Console.WriteLine($"{e.Id}: {e.FullName}");
        loggedEmployee = Input.Number("Employee ID", 1, int.MaxValue);
        if (!people.Any(e => e.Id == loggedEmployee)) { Console.WriteLine("Unknown employee."); return; }
    }
}

while (true)
{
    Console.WriteLine("\n===== MENU =====");
    Console.WriteLine("1. Add asset (Admin/Manager)");
    Console.WriteLine("2. Show all assets");
    Console.WriteLine("3. Update asset (Admin/Manager)");
    Console.WriteLine("4. Delete asset (Admin)");
    Console.WriteLine("5. Search assets");
    Console.WriteLine("6. Filter assets");
    Console.WriteLine("7. Reports / dashboard");
    Console.WriteLine("8. Export TXT / CSV / JSON");
    Console.WriteLine("9. Manage employees / assignments (Admin/Manager)");
    Console.WriteLine("10. Record maintenance (Admin/Manager)");
    Console.WriteLine("0. Exit");
    var action = Input.Number("Choice", 0, 10);
    if (action == 0) break;
    try
    {
        if (role == 3 && action is 1 or 3 or 4 or 8 or 9 or 10)
        {
            Console.WriteLine("Not allowed for Employee role.");
            continue;
        }
        if (role != 1 && action == 4)
        {
            Console.WriteLine("Only Admin can delete assets.");
            continue;
        }
        switch (action)
        {
            case 1: AddAsset(); break;
            case 2: Display(VisibleAssets()); break;
            case 3: UpdateAsset(); break;
            case 4: DeleteAsset(); break;
            case 5: SearchAssets(); break;
            case 6: FilterAssets(); break;
            case 7:
                ReportService.Print(VisibleAssets(), db.Employees.Include(e => e.AssignedAssets).ToList());
                break;
            case 8:
                var format = Input.Number("1 TXT | 2 CSV | 3 JSON", 1, 3);
                Console.WriteLine("Saved to: " + ExportService.Export(service.All(), format));
                break;
            case 9: ManageEmployees(); break;
            case 10: Maintenance(); break;
        }
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine("Could not save data (check unique serial number and required fields): " + ex.GetBaseException().Message);
        db.ChangeTracker.Clear();
    }
    catch (Exception ex) when (ex is IOException or ArgumentException or InvalidOperationException)
    {
        Console.WriteLine("Operation failed: " + ex.Message);
    }
}

List<Asset> VisibleAssets() => role == 3
    ? service.All().Where(a => loggedEmployee.HasValue && a.EmployeeId == loggedEmployee).ToList()
    : service.All();

void Display(IEnumerable<Asset> list)
{
    var items = list.ToList();
    if (items.Count == 0) { Console.WriteLine("No assets found."); return; }
    foreach (var group in items.GroupBy(a => a is ComputerAsset ? "COMPUTERS" : "MOBILE DEVICES"))
    {
        Console.WriteLine($"\n=== {group.Key} ===");
        Console.WriteLine("ID | Type | Brand | Model | Office | Purchased | USD | Local | Status");
        foreach (var a in group)
        {
            var status = a.Status(DateTime.Today);
            Console.ForegroundColor = status switch
            {
                "YELLOW" => ConsoleColor.Yellow,
                "RED" or "EXPIRED" => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };
            Console.WriteLine($"{a.Id} | {a.AssetType} | {a.Brand} | {a.ModelName} | {a.Office.Name} | " +
                              $"{a.PurchaseDate:yyyy-MM-dd} | ${a.PurchasePriceUsd:N2} | {a.LocalPrice:N2} {a.Office.Currency} | {status}");
            Console.ResetColor();
        }
    }
}

Office ChooseOffice()
{
    var offices = db.Offices.OrderBy(o => o.Id).ToList();
    foreach (var o in offices) Console.WriteLine($"{o.Id}. {o.Name} ({o.Currency})");
    while (true)
    {
        var id = Input.Number("Office ID", 1, int.MaxValue);
        var office = offices.FirstOrDefault(o => o.Id == id);
        if (office is not null) return office;
        Console.WriteLine("Office not found.");
    }
}

void FillAsset(Asset a, bool update)
{
    a.Brand = Input.Required("Brand", update ? a.Brand : null);
    a.ModelName = Input.Required("Model name", update ? a.ModelName : null);
    a.PurchaseDate = Input.Date("Purchase date", update ? a.PurchaseDate : null);
    a.PurchasePriceUsd = Input.Money("Purchase price", update ? a.PurchasePriceUsd : null);
    a.SerialNumber = Input.Required("Serial number", update ? a.SerialNumber : null);
    a.Username = Input.Optional("Username", a.Username);
    a.WarrantyExpirationDate = Input.Date("Warranty expiration", update ? a.WarrantyExpirationDate : null);
    if (a.WarrantyExpirationDate < a.PurchaseDate)
        throw new ArgumentException("Warranty expiration cannot be before the purchase date.");
    a.OfficeId = ChooseOffice().Id;
}

void AddAsset()
{
    Console.WriteLine("1 Laptop | 2 Desktop | 3 iPhone | 4 Samsung | 5 Nokia | 6 Tablet");
    var choice = Input.Number("Asset type", 1, 6);
    string type = new[] { "Laptop", "Desktop", "iPhone", "Samsung", "Nokia", "Tablet" }[choice - 1];
    Asset asset = choice <= 2 ? new ComputerAsset(type, "", "") : new MobileAsset(type, "", "");
    FillAsset(asset, false);
    service.Add(asset);
    Console.WriteLine($"Asset #{asset.Id} added.");
}

void UpdateAsset()
{
    var a = service.Find(Input.Number("Asset ID to update", 1, int.MaxValue));
    if (a is null) { Console.WriteLine("Not found."); return; }
    Console.WriteLine("Press Enter to keep text/date/price fields. Office selection is required.");
    FillAsset(a, true);
    service.Update();
    Console.WriteLine("Updated.");
}

void DeleteAsset()
{
    var id = Input.Number("Asset ID to delete", 1, int.MaxValue);
    Console.Write("Type YES to confirm: ");
    if (Console.ReadLine() == "YES") Console.WriteLine(service.Delete(id) ? "Deleted." : "Not found.");
    else Console.WriteLine("Cancelled.");
}

void SearchAssets()
{
    var field = Input.Number("1 Brand | 2 Model | 3 Office | 4 Purchase year", 1, 4);
    var term = Input.Required("Search text/year");
    var key = new[] { "brand", "model", "office", "year" }[field - 1];
    var results = service.Search(key, term);
    Display(role == 3 ? results.Where(a => a.EmployeeId == loggedEmployee) : results);
}

void FilterAssets()
{
    var choice = Input.Number("1 Expired | 2 Computers | 3 Mobile | 4 By office", 1, 4);
    var officeId = choice == 4 ? ChooseOffice().Id : 0;
    var filtered = service.Filter(choice, officeId);
    Display(role == 3 ? filtered.Where(a => a.EmployeeId == loggedEmployee) : filtered);
}

void ManageEmployees()
{
    Console.WriteLine("1 Add employee | 2 List employees/assigned assets | 3 Assign or unassign asset");
    var choice = Input.Number("Choice", 1, 3);
    if (choice == 1)
    {
        var e = new Employee
        {
            FullName = Input.Required("Full name"),
            Department = Input.Required("Department"),
            Email = Input.Required("Email")
        };
        if (!e.Email.Contains('@')) { Console.WriteLine("Invalid email."); return; }
        db.Employees.Add(e);
        db.SaveChanges();
        Console.WriteLine($"Employee #{e.Id} added.");
    }
    else if (choice == 2)
    {
        foreach (var e in db.Employees.Include(e => e.AssignedAssets).OrderBy(e => e.FullName))
        {
            Console.WriteLine($"{e.Id}: {e.FullName} | {e.Department} | {e.Email}");
            foreach (var a in e.AssignedAssets) Console.WriteLine($"    {a.Id} - {a.Brand} {a.ModelName}");
        }
    }
    else
    {
        var a = service.Find(Input.Number("Asset ID", 1, int.MaxValue));
        if (a is null) { Console.WriteLine("Asset not found."); return; }
        foreach (var e in db.Employees.OrderBy(e => e.Id)) Console.WriteLine($"{e.Id}: {e.FullName}");
        var id = Input.Number("Employee ID (0 to unassign)", 0, int.MaxValue);
        if (id != 0 && !db.Employees.Any(e => e.Id == id)) { Console.WriteLine("Employee not found."); return; }
        a.EmployeeId = id == 0 ? null : id;
        service.Update();
        Console.WriteLine("Assignment saved.");
    }
}

void Maintenance()
{
    var a = service.Find(Input.Number("Asset ID", 1, int.MaxValue));
    if (a is null) { Console.WriteLine("Not found."); return; }
    a.LastMaintenanceDate = Input.Date("Last maintenance", a.LastMaintenanceDate);
    a.NextMaintenanceDate = Input.Date("Next maintenance", a.NextMaintenanceDate);
    if (a.NextMaintenanceDate < a.LastMaintenanceDate)
    {
        Console.WriteLine("Next maintenance cannot be before last maintenance.");
        db.Entry(a).Reload();
        return;
    }
    a.MaintenanceNotes = Input.Optional("Maintenance notes", a.MaintenanceNotes);
    service.Update();
    Console.WriteLine("Maintenance saved.");
}
