namespace ImportExportCsvAPI.Application.DTOs
{
    public record CsvDto(IFormFile File, bool HasHeader);
}
