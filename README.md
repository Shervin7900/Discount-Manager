# Discount Manager (Modular Monolith)

A reference application built with **.NET 8** and a **Modular Monolith** architecture. This project demonstrates how to build a scalable e-commerce backend with loosely coupled modules, CQRS principles, and rich features like Catalog, Discounts, Ordering, Payment processing (Parbad), and Redis-backed Search.

## 🏗️ Architecture

The solution is structured as a Modular Monolith, where each business capability is encapsulated in its own module.

### Core Modules
- **Bootstrapper**: The Web API Host that composes and runs all modules.
- **SharedKernel**: Common building blocks (Domain, DDD base classes, Utilities).
- **Catalog**: Manages products, categories, and pricing.
- **Shops**: Manages different shop entities.
- **Discount**: Handles coupons and promotional logic.
- **Inventory**: Tracks stock levels.
- **Basket**: Uses **Redis** for performant shopping cart management.
- **Ordering**: Handles checkout and order placement.
- **Payment**: Integrates with **Parbad** library for payment processing (SQL Server storage).
- **Search**: Provides search capabilities with **Redis** indexing and priority sorting.
- **File-Storage**: Handles file uploads.

### Key Technologies
- **.NET 8** (ASP.NET Core Web API)
- **Entity Framework Core** (SQL Server)
- **Redis** (Basket Data & Search Index)
- **Parbad** (Payment Gateway Abstraction)
- **MediatR** (In-process messaging)
- **Swagger / OpenAPI** (API Documentation)
- **FluentValidation**

## 🚀 Getting Started

### Prerequisites

- **.NET 8 SDK**
- **SQL Server** (LocalDB or Docker or Azure SQL)
- **Redis** (Running on `localhost:6379`)

### Installation & Setup

1.  **Clone the repository**
    ```bash
    git clone https://github.com/yourusername/DiscountManager.git
    cd DiscountManager
    ```

2.  **Configure Database Connection Strings**
    Update `src/Bootstrapper/DiscountManager.Bootstrapper/appsettings.json` with your SQL Server connection strings for each module context (`CatalogDb`, `ShopsDb`, `DiscountDb`, `OrderingDb`, `PaymentDb`, `InventoryDb`) and Redis connection (`Redis`).

3.  **Run Migrations**
    This project uses separate DbContexts for each module. You can apply all migrations using the provided script (Windows):
    ```powershell
    .\run_migrations.bat
    ```
    Or manually for each module:
    ```bash
    dotnet ef database update -c CatalogDbContext -p src/Modules/DiscountManager.Modules.Catalog -s src/Bootstrapper/DiscountManager.Bootstrapper
    # Repeat for Shops, Discount, Payment, Ordering, Inventory...
    ```

4.  **Run the Application**
    ```bash
    dotnet run --project src/Bootstrapper/DiscountManager.Bootstrapper
    ```

### Usage

Once the application is running (e.g., on `https://localhost:7216`):

1.  **Swagger UI (API Gateway)**
    Navigate to `https://localhost:7216/swagger` to see all available endpoints across all modules.

2.  **Data Seeding**
    On startup, the application automatically seeds:
    - **Shops**: TechWorld, FashionHub, HomeDepotClone.
    - **Products**: Gaming Laptops, Keyboards, Shoes, etc. (Some with `SalesPrice` discounts).
    - **Coupons**: WELCOME10, SUMMER20, BLACKFRIDAY.
    - **Search Index**: Products are automatically indexed in Redis.

3.  **Example Workflow (Checkout)**
    - **Browse Catalog**: `GET /api/catalog`
    - **Search**: `GET /api/search?q=laptop` (Discounted items appear first!)
    - **Add to Basket**: `POST /api/basket/{userId}/items`
    - **Checkout**: `POST /api/checkout/{userId}?callbackUrl=...`
    - **Pay**: Follow the returned Payment URL (Parbad Virtual Gateway).
    - **Verify**: `GET /api/payment/verify/{trackId}`

## 🧪 Testing

You can use the provided Swagger UI or Postman to test the endpoints.
- **Catalog**: `GET /api/catalog`
- **Discounts**: `GET /api/discount`
- **Basket**: `POST /api/basket/{id}` (requires Redis)

## 📂 Project Structure

```text
src/
  Bootstrapper/       # API Host & Composition Root
  Shared/             # Shared Kernel & Abstractions
  Modules/            # Business Modules
    Catalog/          # Product Management
    Shops/            # Shop Management
    Discount/         # Coupon Logic
    Inventory/        # Stock Tracking
    Basket/           # Shopping Cart (Redis)
    Ordering/         # Order Processing
    Payment/          # Payment Gateway (Parbad)
    Search/           # Search Service (Redis)
    Storage/          # File Uploads
```

## ⚠️ Known Issues / Notes

- **Redis Dependency**: The Basket and Search modules require Redis. If Redis is unavailable, the application will start, but these features will log warnings and may not function correctly (Search falls back to a No-Op service).
- **Security**: Authentication/Authorization (OAuth2/Identity) is planned for future implementation. Currently, APIs are open.

## License

MIT
