dotnet ef migrations add InitialCreate -c CatalogDbContext -p src/Modules/DiscountManager.Modules.Catalog -s src/Bootstrapper/DiscountManager.Bootstrapper
dotnet ef database update -c CatalogDbContext -p src/Modules/DiscountManager.Modules.Catalog -s src/Bootstrapper/DiscountManager.Bootstrapper

dotnet ef migrations add InitialCreate -c DiscountDbContext -p src/Modules/DiscountManager.Modules.Discount -s src/Bootstrapper/DiscountManager.Bootstrapper
dotnet ef database update -c DiscountDbContext -p src/Modules/DiscountManager.Modules.Discount -s src/Bootstrapper/DiscountManager.Bootstrapper

dotnet ef migrations add InitialCreate -c PaymentDbContext -p src/Modules/DiscountManager.Modules.Payment -s src/Bootstrapper/DiscountManager.Bootstrapper
dotnet ef database update -c PaymentDbContext -p src/Modules/DiscountManager.Modules.Payment -s src/Bootstrapper/DiscountManager.Bootstrapper

dotnet ef migrations add InitialCreate -c OrderingDbContext -p src/Modules/DiscountManager.Modules.Ordering -s src/Bootstrapper/DiscountManager.Bootstrapper
dotnet ef database update -c OrderingDbContext -p src/Modules/DiscountManager.Modules.Ordering -s src/Bootstrapper/DiscountManager.Bootstrapper

dotnet ef migrations add InitialCreate -c ShopsDbContext -p src/Modules/DiscountManager.Modules.Shops -s src/Bootstrapper/DiscountManager.Bootstrapper
dotnet ef database update -c ShopsDbContext -p src/Modules/DiscountManager.Modules.Shops -s src/Bootstrapper/DiscountManager.Bootstrapper

dotnet ef migrations add InitialCreate -c InventoryDbContext -p src/Modules/DiscountManager.Modules.Inventory -s src/Bootstrapper/DiscountManager.Bootstrapper
dotnet ef database update -c InventoryDbContext -p src/Modules/DiscountManager.Modules.Inventory -s src/Bootstrapper/DiscountManager.Bootstrapper
