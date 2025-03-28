<div align="center">

# CSV - Import & Export API

</div>

<div>
CsvImportExport is an API developed in .NET 8 to facilitate the import, export and processing of CSV files. With support for automatic validations, ZIP compression and efficient data manipulation. The project follows Clean Architecture principles, separating responsibilities into well-defined layers. In addition, it uses Design Patterns to ensure scalability and efficient maintenance.
</div>

### 🔹 Main Features:
```
✅ Import CSV files with validation.
✅ Automatic TXT to CSV conversion.
✅ Export multiple CSV files compressed in ZIP.
```

### 🚀 Technologies:

🔹 Back-end (.NET)
- .NET 8 – Main framework for API development.
- ASP.NET Core Web API – For building RESTful APIs.
- C# – Programming language.

🔹 Validation & Documentation
- FluentValidation – Data validation.
- Swagger – API documentation.

🔹 Logging & Dependency Management
- ILogger – Structured and centralized logging.
- IServiceProvider – Dynamic dependency injection.

🔹 File Handling & Processing
- CsvHelper – Reading and writing CSV files.
- System.IO – File operations (reading/writing).
- System.IO.Compression (ZipArchive) – Compressing and exporting files.

🔹 Architecture
- Clean Architecture Principles Used – Separation between API, services, and data handling.

🔹 Design Patterns Used
- Dependency Injection (DI) – To decouple components and improve maintainability.
- Result Pattern – Standardized error handling and validation responses.

 ### 🎯 Final result:
 ![Swagger](https://i.imgur.com/iKX2dnH.png)
