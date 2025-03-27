using ImportExportCsvAPI.Domain.Abstractions;
using ImportExportCsvAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ImportExportCsvAPI.Application.Services
{
    public class CsvService : ICsvService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileValidationService _fileValidationService;
        private readonly ILogger<CsvService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CsvService(IFileStorageService fileStorageService, IFileValidationService fileValidationService, ILogger<CsvService> logger, IServiceProvider serviceProvider)
        {
            _fileStorageService = fileStorageService;
            _fileValidationService = fileValidationService;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<Result<string>> QueueImport(List<Dictionary<string, object>> records, string fileName)
        {
            _logger.LogInformation("Queueing file import: {FileName}", fileName);

            try
            {
                var filePath = await _fileStorageService.SaveToTempFileAsync(records, fileName);
                if (!filePath.IsSuccess)
                {
                    return Result<string>.Failure(filePath.Errors);
                }

                _serviceProvider.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(() =>
                {
                    _logger.LogInformation("Application is shutting down. Stopping background process.");
                });

                _serviceProvider.GetRequiredService<IBackgroundTaskQueue>().QueueBackgroundWorkItem(async token =>
                {
                    _logger.LogInformation("Processing file: {FilePath}", filePath.Data);
                    await Task.Delay(500, token); // Simulating a small processing
                    _logger.LogInformation("Processing completed: {FilePath}", filePath.Data);
                });

                return Result<string>.Success($"File saved in: {filePath.Data}");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(["Failed to write file."]);
            }
        }

        public async Task<Result<PhysicalFileResult>> GenerateCsv()
        {
            var txtFiles = _fileStorageService.GetTxtFiles();
            if (!txtFiles.IsSuccess)
            {
                return Result<PhysicalFileResult>.Failure(txtFiles.Errors);
            }

            var filesValid = _fileValidationService.ValidateFileExists(txtFiles.Data);
            if (!filesValid.IsSuccess)
            {
                return Result<PhysicalFileResult>.Failure(filesValid.Errors);
            }

            var zipFilePath = await _fileStorageService.CreateZipFile(txtFiles.Data);
            if (!zipFilePath.IsSuccess)
            {
                return Result<PhysicalFileResult>.Failure(zipFilePath.Errors);
            }

            var fileResult = new PhysicalFileResult(zipFilePath.Data, "application/zip")
            {
                FileDownloadName = zipFilePath.Data,
                FileName = Path.GetFileName(zipFilePath.Data)
            };

            return Result<PhysicalFileResult>.Success(fileResult);
        }
    }
}
