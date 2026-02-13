# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["DiscountManager.sln", "./"]
COPY ["src/Gateway/DiscountManager.Gateway/DiscountManager.Gateway.csproj", "src/Gateway/DiscountManager.Gateway/"]

# Restore dependencies
RUN dotnet restore "src/Gateway/DiscountManager.Gateway/DiscountManager.Gateway.csproj"

# Copy source code
COPY ["src/Gateway/DiscountManager.Gateway/", "src/Gateway/DiscountManager.Gateway/"]

# Build
WORKDIR "/src/src/Gateway/DiscountManager.Gateway"
RUN dotnet build "DiscountManager.Gateway.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "DiscountManager.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DiscountManager.Gateway.dll"]
