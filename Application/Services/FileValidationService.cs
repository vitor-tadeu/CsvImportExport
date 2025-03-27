using ImportExportCsvAPI.Domain.Abstractions;
using ImportExportCsvAPI.Domain.Interfaces;

namespace ImportExportCsvAPI.Application.Services
{
    public class FileValidationService : IFileValidationService
    {
        public Result<string> ValidateFile(IFormFile file)
        {
            if (file is null)
            {
                return Result<string>.Failure(["Invalid file."]);
            }

            string fileName = Path.GetFileNameWithoutExtension(file.FileName);

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Result<string>.Failure([$"File ({fileName}) must be a CSV."]);
            }

            if (file.Length == 0)
            {
                return Result<string>.Failure([$"File ({fileName}) is empty."]);
            }

            return Result<string>.Success(string.Empty);
        }

        public Result<string> ValidateFileExists(string[] txtFiles)
        {
            if (!txtFiles.Any())
            {
                return Result<string>.Failure(["No TXT files found."]);
            }

            return Result<string>.Success(string.Empty);
        }
    }
}
