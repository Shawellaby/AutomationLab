using System.Text.Json;

namespace AutomationLab;

public record HostMetadataDto(string? MachineName, string? ClientVersion, int? ProcessId);

public record StartExecutionRequest(
    string ExternalExecutionId,
    string TriggerSource,
    string? TriggeredBy,
    DateTime StartUtc,
    HostMetadataDto? Host,
    JsonElement? Parameters
);