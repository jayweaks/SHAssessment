# DME Extractor

A .NET console application that extracts structured medical equipment data from physician notes and sends it to an external API.

## Development Environment

- **IDE**: Visual Studio Code
- **AI Tools**: GitHub Copilot for code assistance and refactoring
- **Platform**: .NET 8.0
- **Testing Framework**: xUnit with Moq for mocking

## Features

- Extracts medical equipment information from physician notes (CPAP, Oxygen Tank, Wheelchair)
- Supports multiple input formats (plain text and JSON)
- Structured logging
- Comprehensive unit tests
- Pretty-printed JSON output for immediate feedback
- Well-defined classes for configuration, models, and services
- Interfaces separate business logic and external dependencies for maintainability
- Dependency injection for flexible service management and testing
- Centralized app configuration via `appsettings.json`
- API endpoint and other settings managed through configuration file


## Project Structure

```
DMEExtractor/
├── Configuration/      # App settings and service configuration
├── Infrastructure/     # External API client
├── Interfaces/         # Service contracts
├── Models/             # Data models with JSON serialization
├── Services/           # Core business logic
└── Program.cs          # Application entry point

DMEExtractor.UnitTests/
├── Configuration/      # Configuration tests
├── Infrastructure/     # API client tests
└── Services/           # Business logic tests

data/
├── input/              # Sample physician notes
└── output/             # Expected output examples
```

## How to Run

### Prerequisites
- .NET 8.0 SDK installed
- Visual Studio Code (optional, for development)

### Running the Application

1. **Clone and navigate to the project:**
   ```bash
   cd SHAssessment
   ```

2. **Run with a specific file:**
   ```bash
   dotnet run --project DMEExtractor data/input/physician_note1.txt
   ```

3. **Run with multiple files:**
   ```bash
   dotnet run --project DMEExtractor data/input/physician_note1.txt data/input/physician_note2.txt
   ```

4. **Run without arguments. Processess all notes in appsettings.json defined BaseInputDirectory (Currently data/input/):**
   ```bash
   dotnet run --project DMEExtractor
   ```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal
```

### Building the Project

```bash
# Build all projects
dotnet build
```

## Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "AppSettings": {
    "ApiEndpoint": "https://alert-api.com/DrExtract",
    "BaseInputDirectory": "../../../../data/input"
  }
}
```

- **ApiEndpoint**: External API endpoint for sending extracted data
- **BaseInputDirectory**: Base directory for input files (relative paths)

## Sample Output

```json
{
  "device": "Oxygen Tank",
  "liters": "2 L",
  "usage": "sleep and exertion",
  "diagnosis": "COPD",
  "ordering_provider": "Dr. Cuddy",
  "patient_name": "Harold Finch",
  "dob": "04/12/1952"
}
```

## Assumptions & Design Decisions

### Assumptions
- Physician notes follow consistent formatting patterns
- Device types are currently limited to CPAP, Oxygen Tank, and Wheelchair
- File paths are relative to the configured base directory
- API endpoint accepts JSON payloads via HTTP POST

### Design Decisions
- **Interfaces and seperation of logic**: Better maintainability
- **Dependency Injection**: For testability and flexibility
- **Async/Await**: Some I/O operations are asynchronous for better performance
- **Structured Logging**: Uses `ILogger<T>`
- **Regex Patterns**: Centralized in constants for maintainability

## Known Limitations

1. **Pattern Matching**: Relies on regex patterns that may not cover all note formats. Very messy and not easily maintainable.
2. **Device Types**: Limited to three predefined device types
5. **API Failure**: Current target endpoint does not work and will fail.

NOTE: Uncomment the await _apiClient.SendEquipmentDataAsync(equipmentData) line in Services/DMEExtractorService.cs to call API.

## Possible Future Improvements

- [ ] Natural language processing for unstructured notes
- [ ] OCR support for scanned documents would be cool
- [ ] Support for additional device types via configuration
- [ ] Add more comprehensive input validation
- [ ] Database storage for extracted data
- [ ] Implement file watching for automatic processing
