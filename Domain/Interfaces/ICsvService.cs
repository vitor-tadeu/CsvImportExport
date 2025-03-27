using ImportExportCsvAPI.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ImportExportCsvAPI.Domain.Interfaces
{
    public interface ICsvService
    {
        Task<Result<string>> QueueImport(List<Dictionary<string, object>> records, string fileName);
        Task<Result<PhysicalFileResult>> GenerateCsv();
    }
}
