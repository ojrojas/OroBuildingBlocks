using System.Diagnostics;

namespace BuildingBlocks.ServiceDefaults.Observability;

/// <summary>Enriches the current activity and log scope with trusted operation identifiers.</summary>
public sealed class OperationTelemetryScopeFactory
{
    public IDisposable BeginScope(
        ILogger logger,
        string? actorId,
        string? tenantId,
        string? correlationId,
        string? causationId)
    {
        var fields = new Dictionary<string, object?>();
        Add(fields, "ActorId", actorId);
        Add(fields, "TenantId", tenantId);
        Add(fields, "CorrelationId", correlationId);
        Add(fields, "CausationId", causationId);

        Activity.Current?.SetTag("orotrading.actor_id", actorId);
        Activity.Current?.SetTag("orotrading.tenant_id", tenantId);
        Activity.Current?.SetTag("orotrading.correlation_id", correlationId);
        Activity.Current?.SetTag("orotrading.causation_id", causationId);

        return logger.BeginScope(fields) ?? EmptyScope.Instance;
    }

    private static void Add(Dictionary<string, object?> fields, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            fields[key] = value;
        }
    }

    private sealed class EmptyScope : IDisposable
    {
        public static EmptyScope Instance { get; } = new();
        public void Dispose() { }
    }
}
