using ImportExportCsvAPI.Application.DTOs;
using ImportExportCsvAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ImportExportCsvAPI.API.Controllers
{
    [ApiController]
    [Route("api/csv")]
    public class CsvController : ControllerBase
    {
        private readonly IFileValidationService _fileValidationService;
        private readonly ICsvReaderService _csvReaderService;
        private readonly ICsvService _csvService;
        private readonly ILogger<CsvController> _logger;

        public CsvController(IFileValidationService fileValidationService, ICsvReaderService csvReaderService, ICsvService csvService, ILogger<CsvController> logger)
        {
            _fileValidationService = fileValidationService;
            _csvReaderService = csvReaderService;
            _csvService = csvService;
            _logger = logger;
        }

        [HttpPost]
        [Route("import")]
        public async Task<IResult> ImportCsv([FromForm] CsvDto csvDto)
        {
            _logger.LogInformation("Receiving file for import.");

            var file = _fileValidationService.ValidateFile(csvDto.File);
            if (!file.IsSuccess)
            {
                _logger.LogWarning("File validation failed: {Errors}", string.Join(", ", file.Errors));
                return Results.BadRequest(file.Errors);
            }

            var records = _csvReaderService.ReadCsv(csvDto);
            if (!records.IsSuccess)
            {
                _logger.LogWarning("CSV validation failed: {Errors}", string.Join(", ", records.Errors));
                return Results.BadRequest(records.Errors);
            }

            var import = await _csvService.QueueImport(records.Data, csvDto.File.FileName);
            if (!import.IsSuccess)
            {
                _logger.LogWarning("Import validation failed: {Errors}", string.Join(", ", import.Errors));
                return Results.BadRequest(import.Errors);
            }

            _logger.LogInformation("File sent for processing successfully.");
            return Results.Ok(import);
        }

        [HttpGet]
        [Route("export")]
        public async Task<IActionResult> ExportCsvAsZip()
        {
            _logger.LogInformation("Starting CSV export process...");

            var file = await _csvService.GenerateCsv();
            if (!file.IsSuccess)
            {
                _logger.LogWarning("CSV exportation failed: {Errors}", string.Join(", ", file.Errors));
                return BadRequest(file.Errors);
            }

            _logger.LogInformation("CSV export successful: {FileName}", file.Data?.FileDownloadName);
            return Ok($"File exported in: {file.Data?.FileDownloadName}");
        }
    }
}
