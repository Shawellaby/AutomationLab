using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationLab;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContextPool<TelemetryDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AutomationLab")));
        var app = builder.Build();
        var api = app.MapGroup("/api/v1/systems/{systemCode}/jobs/{jobCode}/executions");

        // Helper to resolve the JobDefId
        static async Task<long?> ResolveJobDefIdAsync(string systemCode, string jobCode, TelemetryDbContext db)
        {
            return await db.JobDefinitions.AsNoTracking().Where(j => j.JobCode == jobCode && j.System.SystemCode == systemCode && j.IsActive).Select(j => (long?)j.JobDefId)
                .FirstOrDefaultAsync();
        }

// 1. START EVENT
        api.MapPost("/start", async ([FromRoute] string systemCode, [FromRoute] string jobCode, [FromBody] StartExecutionRequest req, TelemetryDbContext db) =>
        {
            var jobDefId = await ResolveJobDefIdAsync(systemCode, jobCode, db);
            if (jobDefId is null) return Results.NotFound(new { message = "System or Job code not registered." });
            var existing = await db.JobExecutions.FirstOrDefaultAsync(e => e.JobDefId == jobDefId.Value && e.ExternalExecutionId == req.ExternalExecutionId);
            if (existing != null)
            {
                // Idempotency: Return existing run without error
                return Results.Ok(new { executionId = existing.ExecutionId, status = existing.Status });
            }

            var now = DateTime.UtcNow;
            var execution = new JobExecution
            {
                JobDefId = jobDefId.Value, ExternalExecutionId = req.ExternalExecutionId, TriggerSource = req.TriggerSource, TriggeredBy = req.TriggeredBy, Status = "Running"
                , StartUtc = req.StartUtc.ToUniversalTime(), HostMachine = req.Host?.MachineName, ClientVersion = req.Host?.ClientVersion, ParametersJson = req.Parameters?.GetRawText()
                , IngestedUtc = now, LastModifiedUtc = now
            };

            db.JobExecutions.Add(execution);
            await db.SaveChangesAsync();
            return Results.Created($"/api/v1/systems/{systemCode}/jobs/{jobCode}/executions/{execution.ExternalExecutionId}", new { executionId = execution.ExecutionId });
        });

// 2. HEARTBEAT EVENT
        api.MapPut("/{externalExecutionId}/heartbeat", async ([FromRoute] string systemCode, [FromRoute] string jobCode, [FromRoute] string externalExecutionId
            , [FromBody] HeartbeatRequest req, TelemetryDbContext db) =>
        {
            var jobDefId = await ResolveJobDefIdAsync(systemCode, jobCode, db);
            if (jobDefId is null) return Results.NotFound();
            // High performance in-place update using EF Core 7+ ExecuteUpdate
            var updated = await db.JobExecutions.Where(e => e.JobDefId == jobDefId.Value && e.ExternalExecutionId == externalExecutionId)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.LastModifiedUtc, DateTime.UtcNow));
            return updated > 0 ? Results.Ok() : Results.NotFound();
        });

// 3. COMPLETE EVENT (Handles late arriving starts gracefully)
        api.MapPut("/{externalExecutionId}/complete", async ([FromRoute] string systemCode, [FromRoute] string jobCode, [FromRoute] string externalExecutionId
            , [FromBody] CompleteExecutionRequest req, TelemetryDbContext db) =>
        {
            var jobDefId = await ResolveJobDefIdAsync(systemCode, jobCode, db);
            if (jobDefId is null) return Results.NotFound();
            var execution = await db.JobExecutions.FirstOrDefaultAsync(e => e.JobDefId == jobDefId.Value && e.ExternalExecutionId == externalExecutionId);
            var now = DateTime.UtcNow;
            if (execution is null)
            {
                // Out-of-order delivery safeguard: Start event was dropped/delayed, create directly
                execution = new JobExecution
                {
                    JobDefId = jobDefId.Value, ExternalExecutionId = externalExecutionId, TriggerSource = "Unknown", StartUtc = req.EndUtc.ToUniversalTime(), // Best-effort fallback
                    IngestedUtc = now
                };
                db.JobExecutions.Add(execution);
            }

            execution.Status = req.Status;
            execution.EndUtc = req.EndUtc.ToUniversalTime();
            execution.OutputMetricsJson = req.OutputMetrics?.GetRawText();
            execution.ErrorCode = req.Error?.Code;
            execution.ErrorMessage = req.Error?.Message;
            execution.StackTrace = req.Error?.StackTrace;
            execution.LastModifiedUtc = now;
            await db.SaveChangesAsync();
            return Results.Ok();
        });
        app.Run();
    }
}