using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutomationLab;

public class TelemetryDbContext : DbContext

{
    public TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : base(options)
    {
    }

    public DbSet<AutomatedSystem> AutomatedSystems => Set<AutomatedSystem>();
    public DbSet<JobDefinition> JobDefinitions => Set<JobDefinition>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AutomatedSystem>(b =>
        {
            b.ToTable("AutomatedSystem");
            b.HasKey(x => x.SystemId);
            b.HasIndex(x => x.SystemCode).IsUnique();
            b.Property(x => x.SystemCode).HasMaxLength(50).IsUnicode(false);
            b.Property(x => x.DisplayName).HasMaxLength(100);
            b.Property(x => x.Environment).HasMaxLength(20).IsUnicode(false);
            b.Property(x => x.CreatedUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<JobDefinition>(b =>
        {
            b.ToTable("JobDefinition");
            b.HasKey(x => x.JobDefId);
            b.HasIndex(x => new { x.SystemId, x.JobCode }).IsUnique();
            b.Property(x => x.JobCode).HasMaxLength(100).IsUnicode(false);
            b.Property(x => x.JobName).HasMaxLength(200);
            b.Property(x => x.ExpectedCronExpression).HasMaxLength(100).IsUnicode(false);
            b.Property(x => x.CreatedUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            b.HasOne(x => x.System).WithMany(x => x.JobDefinitions).HasForeignKey(x => x.SystemId);
        });

        modelBuilder.Entity<JobExecution>(b =>
        {
            b.ToTable("JobExecution");
            b.HasKey(x => x.ExecutionId);

            // Critical composite index for lookups and idempotency
            b.HasIndex(x => new { x.JobDefId, x.ExternalExecutionId }).IsUnique();
            b.HasIndex(x => new { x.JobDefId, x.StartUtc });
            b.Property(x => x.ExternalExecutionId).HasMaxLength(128).IsUnicode(false);
            b.Property(x => x.TriggerSource).HasMaxLength(50).IsUnicode(false);
            b.Property(x => x.TriggeredBy).HasMaxLength(100);
            b.Property(x => x.Status).HasMaxLength(20).IsUnicode(false);
            b.Property(x => x.HostMachine).HasMaxLength(100);
            b.Property(x => x.ClientVersion).HasMaxLength(50).IsUnicode(false);
            b.Property(x => x.ErrorCode).HasMaxLength(50).IsUnicode(false);

            // SQL Server DATETIME2 mapping
            b.Property(x => x.StartUtc).HasPrecision(7);
            b.Property(x => x.EndUtc).HasPrecision(7);
            b.Property(x => x.IngestedUtc).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property(x => x.LastModifiedUtc).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");

            // Database computed duration column
            b.Property(x => x.DurationMs).HasComputedColumnSql("DATEDIFF_BIG(MILLISECOND, StartUtc, EndUtc)", stored: true);
            b.HasOne(x => x.JobDefinition).WithMany(x => x.Executions).HasForeignKey(x => x.JobDefId);
        });
    }
}