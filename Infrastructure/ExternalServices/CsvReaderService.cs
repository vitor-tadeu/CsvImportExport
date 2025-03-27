using CsvHelper;
using CsvHelper.Configuration;
using ImportExportCsvAPI.Application.DTOs;
using ImportExportCsvAPI.Domain.Abstractions;
using ImportExportCsvAPI.Domain.Interfaces;
using System.Globalization;
using System.Text;

namespace ImportExportCsvAPI.Infrastructure.ExternalServices
{
    public class CsvReaderService : ICsvReaderService
    {
        public Result<List<Dictionary<string, object>>> ReadCsv(CsvDto csvDto)
        {
            using var reader = CreateCsvReader(csvDto, out var csv);
            if (csv is null)
            {
                return Result<List<Dictionary<string, object>>>.Failure(["Failed to initialize CSV reader."]);
            }

            var records = ReadCsvRecords(csvDto, csv);
            if (!records.IsSuccess)
            {
                return Result<List<Dictionary<string, object>>>.Failure(records.Errors);
            }

            return Result<List<Dictionary<string, object>>>.Success(records.Data);
        }

        private static StreamReader CreateCsvReader(CsvDto csvDto, out CsvReader csv)
        {
            var stream = csvDto.File.OpenReadStream();
            var reader = new StreamReader(stream, Encoding.UTF8);

            csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = csvDto.HasHeader
            });

            if (csvDto.HasHeader)
            {
                csv.Read();
                csv.ReadHeader();
            }

            return reader;
        }

        private static Result<List<Dictionary<string, object>>> ReadCsvRecords(CsvDto csvDto, CsvReader csv)
        {
            var records = csv.GetRecords<dynamic>().ToList();
            if (!records.Any())
            {
                return Result<List<Dictionary<string, object>>>.Failure(["The CSV file has no records."]);
            }

            var formattedRecords = new List<Dictionary<string, object>>();
            if (csvDto.HasHeader)
            {
                formattedRecords.Add(csv.HeaderRecord.ToDictionary(header => header, header => (object)header));
            }

            var processedRecords = ProcessCsvRecords(records, formattedRecords);
            if (!processedRecords.IsSuccess)
            {
                return Result<List<Dictionary<string, object>>>.Failure(processedRecords.Errors);
            }

            return Result<List<Dictionary<string, object>>>.Success(formattedRecords);
        }

        private static Result<string> ProcessCsvRecords(IEnumerable<dynamic> records, List<Dictionary<string, object>> formattedRecords)
        {
            int rowIndex = 1;

            foreach (var record in records)
            {
                var recordDict = (IDictionary<string, object>)record;
                if (recordDict is null || !recordDict.Keys.Any())
                {
                    return Result<string>.Failure(["The CSV file has an invalid format."]);
                }

                var validRecord = ValidateRecord(recordDict, rowIndex);
                if (!validRecord.IsSuccess)
                {
                    return Result<string>.Failure(validRecord.Errors);
                }

                formattedRecords.Add(new Dictionary<string, object>(recordDict));
                rowIndex++;
            }

            return Result<string>.Success(string.Empty);
        }

        private static Result<string> ValidateRecord(IDictionary<string, object> recordDict, int rowIndex)
        {
            if (recordDict.Values.All(value => string.IsNullOrWhiteSpace(value?.ToString())))
            {
                return Result<string>.Failure([$"Error in row {rowIndex}: The row is empty."]);
            }

            int colIndex = 1;
            string columnLetter;

            foreach (var value in recordDict.Values)
            {
                columnLetter = ConvertToColumnLetter(colIndex);

                if (string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    return Result<string>.Failure([$"Error in row {rowIndex}, column {columnLetter}: The field is required and cannot be empty."]);
                }

                colIndex++;
            }

            return Result<string>.Success(string.Empty);
        }

        private static string ConvertToColumnLetter(int colIndex)
        {
            var sb = new StringBuilder();
            while (colIndex > 0)
            {
                colIndex--;
                sb.Insert(0, (char)('A' + colIndex % 26));
                colIndex /= 26;
            }
            return sb.ToString();
        }
    }
}
