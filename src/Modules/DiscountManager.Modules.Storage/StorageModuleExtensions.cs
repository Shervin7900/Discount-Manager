using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DiscountManager.Modules.Storage.Infrastructure;

namespace DiscountManager.Modules.Storage;

public static class StorageModuleExtensions
{
    public static IServiceCollection AddStorageModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        return services;
    }
}
