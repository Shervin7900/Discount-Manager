dotnet ef migrations add AddSalesPriceToProduct -c CatalogDbContext -p src/Modules/DiscountManager.Modules.Catalog -s src/Bootstrapper/DiscountManager.Bootstrapper
dotnet ef database update -c CatalogDbContext -p src/Modules/DiscountManager.Modules.Catalog -s src/Bootstrapper/DiscountManager.Bootstrapper
