using CsvHelper;
using CsvHelper.Configuration;
using ImportExportCsvAPI.Domain.Abstractions;
using ImportExportCsvAPI.Domain.Interfaces;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace ImportExportCsvAPI.Application.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _baseFolderPath;

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
            _baseFolderPath = Path.Combine(Path.GetTempPath(), "ImportExportCsvAPI");
            EnsureDirectoryExists();
        }

        public async Task<Result<string>> SaveToTempFileAsync(List<Dictionary<string, object>> records, string fileName)
        {
            string line;
            string filePath = Path.Combine(_baseFolderPath, $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}.txt");

            try
            {
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
                foreach (var record in records)
                {
                    line = string.Join(",", record.Values);
                    await writer.WriteLineAsync(line);
                }

                if (!File.Exists(filePath))
                {
                    return Result<string>.Failure(["Error saving temporary file."]);
                }

                return Result<string>.Success(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving temporary file: {FilePath}", filePath);
                return Result<string>.Failure(["Error saving temporary file."]);
            }
        }

        public Result<string[]> GetTxtFiles()
        {
            try
            {
                return Result<string[]>.Success(Directory.GetFiles(_baseFolderPath, "*.txt"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TXT files from directory: {Directory}", _baseFolderPath);
                return Result<string[]>.Failure(["Error retrieving TXT files."]);
            }
        }

        public async Task<Result<string>> CreateZipFile(IEnumerable<string> txtFiles)
        {
            string zipFilePath = Path.Combine(_baseFolderPath, $"{Guid.NewGuid()}.zip");

            try
            {
                using (var zipStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var txtFile in txtFiles)
                    {
                        await AddFileToZipAsync(txtFile, archive);
                    }
                }

                if (!File.Exists(zipFilePath))
                {
                    return Result<string>.Failure(["Error creating ZIP file."]);
                }

                return Result<string>.Success(zipFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ZIP file {ZipFilePath}", zipFilePath);
                return Result<string>.Failure(["Error creating ZIP file."]);
            }
        }

        private static async Task AddFileToZipAsync(string txtFile, ZipArchive archive)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(txtFile) + ".csv";
            using var entryStream = archive.CreateEntry(fileNameWithoutExtension).Open();
            using var writer = new StreamWriter(entryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            var lines = await File.ReadAllLinesAsync(txtFile);
            if (!lines.Any())
            {
                return;
            }

            var headers = lines[0].Split(',');
            csv.WriteField(headers);
            await csv.NextRecordAsync();

            foreach (var line in lines.Skip(1))
            {
                csv.WriteField(line.Split(';'));
                await csv.NextRecordAsync();
                await csv.FlushAsync();
            }
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_baseFolderPath))
            {
                Directory.CreateDirectory(_baseFolderPath);
            }
        }
    }
}
