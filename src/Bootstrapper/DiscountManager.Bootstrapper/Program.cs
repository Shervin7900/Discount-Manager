using DiscountManager.Modules.Catalog;
using DiscountManager.Modules.Shops;
using DiscountManager.Modules.Discount;
using DiscountManager.Modules.Inventory;
using DiscountManager.Modules.Basket;
using DiscountManager.Modules.Ordering;
using DiscountManager.Modules.Storage;
using DiscountManager.Modules.Search;
using DiscountManager.Modules.Payment;
using DiscountManager.Modules.Identity;
using DiscountManager.Modules.Customer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MediatR;
using DiscountManager.Bootstrapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
// Controllers are added below with ApplicationParts
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Modules
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddShopsModule(builder.Configuration);
builder.Services.AddDiscountModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);
builder.Services.AddBasketModule(builder.Configuration);
builder.Services.AddOrderingModule(builder.Configuration);
builder.Services.AddStorageModule(builder.Configuration);
builder.Services.AddSearchModule(builder.Configuration);
builder.Services.AddPaymentModule(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCustomerModule(builder.Configuration);

// Configure Authentication (JWT + Cookie)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJwtTokenGenerationMinimum32Characters!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "DiscountManager",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "DiscountManagerClients",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    
    // Support token from cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Token = context.Request.Cookies["AuthToken"];
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie();

// Add Controllers and Register Module Assemblies for Swagger Discovery
builder.Services.AddControllers()
    .AddApplicationPart(typeof(DiscountManager.Modules.Catalog.CatalogModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Shops.ShopsModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Discount.DiscountModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Inventory.InventoryModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Basket.BasketModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Ordering.OrderingModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Storage.StorageModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Search.SearchModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Payment.PaymentModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Identity.IdentityModuleExtensions).Assembly)
    .AddApplicationPart(typeof(DiscountManager.Modules.Customer.CustomerModuleExtensions).Assembly);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly)); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // For Storage
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllers();

// Seed Data
await DataSeeder.SeedAsync(app.Services);

app.Run();

namespace DiscountManager.Bootstrapper
{
    public partial class Program { }
}
