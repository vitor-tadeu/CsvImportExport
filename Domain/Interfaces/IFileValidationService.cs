using ImportExportCsvAPI.Domain.Abstractions;

namespace ImportExportCsvAPI.Domain.Interfaces
{
    public interface IFileValidationService
    {
        Result<string> ValidateFile(IFormFile file);
        Result<string> ValidateFileExists(string[] txtFiles);
    }
}
