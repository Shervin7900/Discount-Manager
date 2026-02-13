using Microsoft.Extensions.Configuration;

namespace DiscountManager.Modules.Storage.Infrastructure;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storagePath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _storagePath = configuration["Storage:Path"] ?? "uploads";
        _baseUrl = configuration["Storage:BaseUrl"] ?? "/uploads/";
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> SaveFileAsync(string fileName, Stream content)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        using var fileStream = new FileStream(filePath, FileMode.Create);
        await content.CopyToAsync(fileStream);
        
        return GetFileUrl(fileName);
    }

    public string GetFileUrl(string fileName)
    {
        return $"{_baseUrl.TrimEnd('/')}/{fileName}";
    }
}
