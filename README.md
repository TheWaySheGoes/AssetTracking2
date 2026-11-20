Smart Asset Tracking System
A C# .NET 10 console application for tracking company IT assets across multiple offices. Built as a learning project covering object-oriented programming, Entity Framework Core, SQLite, LINQ, CRUD operations, and reporting.
Project type: Console application (not an ASP.NET Core website or REST API).

Features
- Asset registration and CRUD: Add, list, edit, and delete laptops, desktops, iPhones, Samsung/Nokia devices, and tablets.
- Asset information: Brand, model, serial number, purchase date, USD price, office, optional username/employee, warranty expiration, and maintenance details.
- Lifecycle status: Three-year asset lifespan with color-coded warnings and an expired state.
- Global offices: Sweden, USA, Germany, and Turkey, with local price conversion using fixed example exchange rates.
- Search and filters: Brand, model, office, purchase year, expired assets, computers, and mobile devices.
- Reports: Asset totals and value per office, approaching end-of-life assets, most expensive assets, assignments, and common asset types.
- Exports: TXT, CSV, and JSON reports in an Exports/ folder.
- Employee assignments and maintenance: Manage employees, assign assets, and record maintenance dates/notes.
- Simulated user roles: Admin, Manager, and Employee, with menu-level access restrictions. This is not secure authentication.
Tech stack
Component	Technology
Language	C#
Framework	.NET 10 (net10.0)
Application	Console App
ORM	Entity Framework Core 10
Database	SQLite
Querying	LINQ
Schema management	EF Core Migrations


Requirements
- .NET 10 SDK
- Visual Studio 2026 with the .NET desktop development workload, or another editor with the .NET SDK
- EF Core CLI tool (dotnet-ef)
No separate database server is required. SQLite stores the database in a local file.
Setup and run
1. Clone or download the repository.
2. Open the folder containing AssetTracking.csproj (or open that project in Visual Studio).
3. Open a terminal in the directory containing AssetTracking.csproj.
4. Restore packages:
   dotnet restore
5. Install the EF Core command-line tool if needed:
   dotnet tool install --global dotnet-ef --version 10.0.0
   If already installed, use dotnet tool update --global dotnet-ef instead. Keep your EF CLI and NuGet versions on a compatible EF Core 10 release; the included project initially pins the NuGet packages to 10.0.0.
6. Create the initial migration only if the repository does not already contain a Migrations/ folder:
   dotnet ef migrations add InitialCreate
7. Create/update the database:
   dotnet ef database update
8. Run:
   dotnet run
   You can also start it in Visual Studio with Ctrl+F5.
On first run, the application seeds four offices. It asks you to select a simulated role, then displays the numbered menu.
Creating the project manually in Visual Studio
If you are copying source files rather than cloning the repository, create a Console App (C#), target .NET 10, and use the project/namespace AssetTracking. Copy in the source folders and the provided AssetTracking.csproj (including its NuGet package references). Run the migration commands above before launching.
Project structure
AssetTracking/
├── Data/
│   ├── AssetDbContext.cs       # EF Core model and SQLite configuration
│   └── SeedData.cs             # Initial offices and sample exchange rates
├── Helpers/
│   └── Input.cs                # Console input validation
├── Models/
│   ├── Asset.cs                # Asset base class; computer/mobile subclasses
│   ├── Employee.cs
│   └── Office.cs
├── Reports/
│   ├── ExportService.cs
│   └── ReportService.cs
├── Services/
│   └── AssetService.cs         # CRUD, queries, searching/filtering
├── Program.cs                 # Startup and menu workflow
├── AssetTracking.csproj
└── README.md
EF Core generates Migrations/ locally when you run dotnet ef migrations add InitialCreate.
Database and business rules
- The EF Core context is AssetDbContext and the SQLite connection uses Data Source=assets.db.
- The database file is created in the current working directory (normally the project folder when run from Visual Studio).
- Serial numbers have a unique index.
- Assets use an inheritance model: Asset is the base class; ComputerAsset and MobileAsset are subclasses.
- End of life is three years after purchase date.
- The assignment's thresholds overlap. The implementation uses YELLOW when less than three months remain, RED when three to six months remain, EXPIRED after end of life, and NORMAL otherwise.
- Office exchange rates in Data/SeedData.cs are fixed educational examples, not live financial rates. Local prices are calculated from the stored USD price and office rate.
Menu overview
1. Add asset (Admin/Manager)
2. Show assets
3. Update asset (Admin/Manager)
4. Delete asset (Admin)
5. Search assets
6. Filter assets
7. Reports / dashboard
8. Export TXT / CSV / JSON
9. Manage employees / assignments (Admin/Manager)
10. Record maintenance (Admin/Manager)
11. Exit
Employees see assigned assets when a matching employee record has been selected. The simulated roles are intended only for demonstration and must not be treated as production access control.
Troubleshooting
Issue	Suggested fix
No project was found	Change to the directory containing AssetTracking.csproj, or pass --project <path-to-csproj>.
Microsoft.EntityFrameworkCore namespace missing	Run dotnet restore, and check the EF packages in .csproj.
UseSqlite missing	Ensure Microsoft.EntityFrameworkCore.Sqlite is referenced and using Microsoft.EntityFrameworkCore; is present.
Startup project lacks Microsoft.EntityFrameworkCore.Design	Restore/install the Design package in the startup project.
No EF Core migration found	Run dotnet ef migrations add InitialCreate, then dotnet ef database update.
EF tools/runtime version warning	Update dotnet-ef to a compatible EF Core 10 patch version; warnings alone do not necessarily block execution.
Database tables missing	Run dotnet ef database update against the intended project and working directory.


Scope and limitations
This is a course mini-project rather than a production asset management service. It does not include a web frontend, REST API, real authentication, a live exchange-rate API, or automated tests. It uses synchronous EF Core operations and menu-based workflows by design.

The code was assembled for the assignment; verify it by building, running migrations, and exercising the menu in your own environment.
