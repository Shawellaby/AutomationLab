# AutomationLab

AutomationLab is a lightweight ASP.NET Core service for collecting and tracking automation job telemetry. It provides HTTP endpoints for registering job execution lifecycle events such as start, heartbeat, and completion, backed by SQL Server through Entity Framework Core.

## Features

- Track automation job executions by system and job code
- Record execution start, heartbeat, and completion events
- Store execution metadata such as trigger source, host information, parameters, metrics, and errors
- Idempotent start handling for duplicate execution events
- Graceful handling of out-of-order completion events
- SQL Server persistence using Entity Framework Core
- Minimal API design for simple integration with automation clients

## Tech Stack

- .NET 10
- ASP.NET Core
- Entity Framework Core
- SQL Server / LocalDB
- C# 14

## Project Structure
```text
AutomationLab/
    ├── Program.cs # Application entry point and API endpoint mappings
    ├── TelemetryDbContext.cs # EF Core database context and model configuration
    ├── AutomatedSystem.cs # Registered automation system model
    ├── JobDefinition.cs # Automation job definition model
    ├── JobExecution.cs # Job execution telemetry model
    ├── StartExecutionRequest.cs # Start event request DTO
    ├── HeartbeatRequest.cs # Heartbeat event request DTO
    ├── CompleteExecutionRequest.cs # Completion event request DTO
    ├── ErrorDetailDto.cs # Error detail DTO
    ├── HostMetadataDto.cs # Host metadata DTO
    ├── appsettings.json # Application configuration
    └── AutomationLab.csproj # Project file
```


## Getting Started

### Prerequisites

Make sure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- SQL Server or SQL Server LocalDB

### Clone the Repository
```bash
 git clone <repository-url> cd AutomationLab
```


### Configure the Database

The default connection string uses SQL Server LocalDB:
```json
{ "ConnectionStrings": { "AutomationLab": "Server=(localdb)\MSSQLLocalDB;Database=AutomationLab;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;" } }
```


Update `appsettings.json` if you want to use a different SQL Server instance.

### Restore Dependencies
```bash
 dotnet restore
```


### Run the Application

```bash
 dotnet run
```


The application will start locally. The exact port is shown in the console output.

## API Overview

Base route:

```text
 /api/v1/systems/{systemCode}/jobs/{jobCode}/executions
```


### Start an Execution
```http
 POST /api/v1/systems/{systemCode}/jobs/{jobCode}/executions/start
```


Records the beginning of a job execution.

Example request:
```json
 { "externalExecutionId": "run-2026-09-19-001", "triggerSource": "Scheduler", "triggeredBy": "NightlyBatch", "startUtc": "2026-09-19T01:00:00Z", "host": { "machineName": "worker-01", "clientVersion": "1.0.0" }, "parameters": { "region": "us-east", "mode": "full" } }
```


Example response:
```json
 { "executionId": 123 }
```


### Send a Heartbeat
```http
 PUT /api/v1/systems/{systemCode}/jobs/{jobCode}/executions/{externalExecutionId}/heartbeat
```


Updates the last-modified timestamp for a running execution.

Example request:
```json 
{}
```


### Complete an Execution
```http
 PUT /api/v1/systems/{systemCode}/jobs/{jobCode}/executions/{externalExecutionId}/complete
```


Marks an execution as completed, failed, cancelled, or another terminal status.

Example request:
```json
 { "status": "Succeeded", "endUtc": "2026-09-19T01:15:30Z", "outputMetrics": { "recordsProcessed": 25000, "filesCreated": 4 }, "error": null }
```

Example failure request:
```json
 { "status": "Failed", "endUtc": "2026-09-19T01:07:12Z", "outputMetrics": { "recordsProcessed": 8400 }, "error": { "code": "IMPORT_TIMEOUT", "message": "The import operation timed out.", "stackTrace": "..." } }
```


## Data Model Summary

AutomationLab stores telemetry using three main concepts:

| Entity | Description |
| --- | --- |
| `AutomatedSystem` | Represents a registered automation system or application |
| `JobDefinition` | Represents a known job within a system |
| `JobExecution` | Represents a single execution/run of a job |

Each job execution is uniquely identified by the combination of:
```text
 JobDefId + ExternalExecutionId
```

This allows clients to safely retry start events without creating duplicate execution records.

## Typical Execution Flow
```text
Client starts job
        ↓
POST /start
        ↓
Client periodically reports liveness
        ↓
PUT /{externalExecutionId}/heartbeat
        ↓
Client finishes job
        ↓
PUT /{externalExecutionId}/complete
```


## Development Notes

### Build
```bash
 dotnet build
```

### Run
```bash
 dotnet run
```


### Configuration

Application settings are managed through:
```text
 appsettings.json appsettings.Development.json
```


For local development, prefer overriding sensitive or machine-specific values in `appsettings.Development.json`.

## Roadmap Ideas

Potential future improvements:

- Add OpenAPI/Swagger endpoint documentation
- Add authentication and authorization
- Add database migrations
- Add dashboard views for execution history
- Add job status querying endpoints
- Add automated tests
- Add structured logging and observability integrations

## License

GNU General Public License v3.0
