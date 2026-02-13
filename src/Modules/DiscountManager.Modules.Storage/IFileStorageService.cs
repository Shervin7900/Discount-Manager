namespace DiscountManager.Modules.Storage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string fileName, Stream content);
    string GetFileUrl(string fileName);
}
