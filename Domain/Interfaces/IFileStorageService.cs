using ImportExportCsvAPI.Domain.Abstractions;

namespace ImportExportCsvAPI.Domain.Interfaces
{
    public interface IFileStorageService
    {
        Task<Result<string>> SaveToTempFileAsync(List<Dictionary<string, object>> records, string fileName);
        Result<string[]> GetTxtFiles();
        Task<Result<string>> CreateZipFile(IEnumerable<string> txtFiles);
    }
}
