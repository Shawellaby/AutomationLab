using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutomationLab;

public class JobExecution
{
    public long ExecutionId { get; set; }

    public long JobDefId { get; set; }

    public string ExternalExecutionId { get; set; } = null!;

    public string TriggerSource { get; set; } = null!;

    public string? TriggeredBy { get; set; }

    public string Status { get; set; } = null!; // Running, Succeeded, Failed, Timeout



    public DateTime StartUtc { get; set; }

    public DateTime? EndUtc { get; set; }



    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]

    public long? DurationMs { get; private set; }



    public string? ParametersJson { get; set; }

    public string? OutputMetricsJson { get; set; }



    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? StackTrace { get; set; }



    public string? HostMachine { get; set; }

    public string? ClientVersion { get; set; }

    public DateTime IngestedUtc { get; set; }

    public DateTime LastModifiedUtc { get; set; }

    public JobDefinition JobDefinition { get; set; } = null!;
}