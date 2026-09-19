using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutomationLab;

public class JobDefinition
{
    public long JobDefId { get; set; }

    public int SystemId { get; set; }

    public string JobCode { get; set; } = null!;

    public string JobName { get; set; } = null!;

    public string? ExpectedCronExpression { get; set; }

    public int CadenceToleranceMinutes { get; set; } = 15;

    public int? ExpectedDurationSeconds { get; set; }

    public int? SlaTimeoutSeconds { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedUtc { get; set; }

    public AutomatedSystem System { get; set; } = null!;

    public ICollection<JobExecution> Executions { get; set; } = new List<JobExecution>();
}