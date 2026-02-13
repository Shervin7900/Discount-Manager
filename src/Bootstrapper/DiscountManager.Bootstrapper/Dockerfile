# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and all project files
COPY ["DiscountManager.sln", "./"]
COPY ["src/Bootstrapper/DiscountManager.Bootstrapper/DiscountManager.Bootstrapper.csproj", "src/Bootstrapper/DiscountManager.Bootstrapper/"]
COPY ["src/Shared/DiscountManager.Shared.SharedKernel/DiscountManager.Shared.SharedKernel.csproj", "src/Shared/DiscountManager.Shared.SharedKernel/"]
COPY ["src/Modules/DiscountManager.Modules.Catalog/DiscountManager.Modules.Catalog.csproj", "src/Modules/DiscountManager.Modules.Catalog/"]
COPY ["src/Modules/DiscountManager.Modules.Discount/DiscountManager.Modules.Discount.csproj", "src/Modules/DiscountManager.Modules.Discount/"]
COPY ["src/Modules/DiscountManager.Modules.Inventory/DiscountManager.Modules.Inventory.csproj", "src/Modules/DiscountManager.Modules.Inventory/"]
COPY ["src/Modules/DiscountManager.Modules.Shops/DiscountManager.Modules.Shops.csproj", "src/Modules/DiscountManager.Modules.Shops/"]
COPY ["src/Modules/DiscountManager.Modules.Basket/DiscountManager.Modules.Basket.csproj", "src/Modules/DiscountManager.Modules.Basket/"]
COPY ["src/Modules/DiscountManager.Modules.Ordering/DiscountManager.Modules.Ordering.csproj", "src/Modules/DiscountManager.Modules.Ordering/"]
COPY ["src/Modules/DiscountManager.Modules.Payment/DiscountManager.Modules.Payment.csproj", "src/Modules/DiscountManager.Modules.Payment/"]
COPY ["src/Modules/DiscountManager.Modules.Search/DiscountManager.Modules.Search.csproj", "src/Modules/DiscountManager.Modules.Search/"]
COPY ["src/Modules/DiscountManager.Modules.Storage/DiscountManager.Modules.Storage.csproj", "src/Modules/DiscountManager.Modules.Storage/"]
COPY ["src/Modules/DiscountManager.Modules.Identity/DiscountManager.Modules.Identity.csproj", "src/Modules/DiscountManager.Modules.Identity/"]
COPY ["src/Modules/DiscountManager.Modules.Customer/DiscountManager.Modules.Customer.csproj", "src/Modules/DiscountManager.Modules.Customer/"]

# Restore dependencies
RUN dotnet restore "src/Bootstrapper/DiscountManager.Bootstrapper/DiscountManager.Bootstrapper.csproj"

# Copy all source code
COPY ["src/", "src/"]

# Build
WORKDIR "/src/src/Bootstrapper/DiscountManager.Bootstrapper"
RUN dotnet build "DiscountManager.Bootstrapper.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "DiscountManager.Bootstrapper.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=publish /app/publish .

# Create directories
RUN mkdir -p /app/logs /app/wwwroot/uploads && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DiscountManager.Bootstrapper.dll"]
