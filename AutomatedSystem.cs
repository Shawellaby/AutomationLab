using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutomationLab;

public class AutomatedSystem
{
    public int SystemId { get; set; }

    public string SystemCode { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string Environment { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedUtc { get; set; }

    public ICollection<JobDefinition> JobDefinitions { get; set; } = new List<JobDefinition>();
}