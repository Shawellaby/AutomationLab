using System.Text.Json;

namespace AutomationLab;

public record CompleteExecutionRequest(
    string Status, // 'Succeeded', 'Failed', 'Canceled'
    DateTime EndUtc,
    JsonElement? OutputMetrics,
    ErrorDetailDto? Error
);