using ImportExportCsvAPI.Application.DTOs;
using ImportExportCsvAPI.Domain.Abstractions;

namespace ImportExportCsvAPI.Domain.Interfaces
{
    public interface ICsvReaderService
    {
        Result<List<Dictionary<string, object>>> ReadCsv(CsvDto csvDto);
    }
}
