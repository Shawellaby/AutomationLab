using System.Text.Json;

namespace AutomationLab;

public record HeartbeatRequest(
    DateTime TimestampUtc,
    decimal? ProgressPercentage,
    JsonElement? IntermediateMetrics
);