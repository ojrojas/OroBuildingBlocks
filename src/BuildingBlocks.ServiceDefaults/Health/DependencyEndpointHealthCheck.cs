using System.Data.Common;
using System.Net.Sockets;
using System.Globalization;

namespace BuildingBlocks.ServiceDefaults.Health;

/// <summary>Checks that a configured dependency endpoint accepts TCP connections.</summary>
/// <remarks>
/// This deliberately reports reachability only. Service-specific health remains the responsibility
/// of protocol-aware adapters; the generic check never returns endpoint or credential details.
/// </remarks>
public sealed class DependencyEndpointHealthCheck(string? connectionString, int defaultPort) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetEndpoint(connectionString, defaultPort, out var host, out var port))
        {
            return HealthCheckResult.Unhealthy("Dependency endpoint is not configured.");
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port, linked.Token);
            return HealthCheckResult.Healthy();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is SocketException or OperationCanceledException or ArgumentException)
        {
            return HealthCheckResult.Unhealthy("Dependency endpoint is unavailable.");
        }
    }

    private static bool TryGetEndpoint(string? value, int defaultPort, out string host, out int port)
    {
        host = string.Empty;
        port = defaultPort;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();
        if (candidate.Contains("://", StringComparison.Ordinal) &&
            Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            host = uri.Host;
            port = uri.IsDefaultPort ? defaultPort : uri.Port;
            return host.Length > 0;
        }

        if (candidate.Contains('=') && candidate.Contains(';'))
        {
            try
            {
                var parts = new DbConnectionStringBuilder { ConnectionString = candidate };
                var hostValue = GetValue(parts, "Host") ?? GetValue(parts, "Server") ?? GetValue(parts, "Data Source");
                if (hostValue is null)
                {
                    return false;
                }

                host = hostValue;
                var portValue = GetValue(parts, "Port");
                return portValue is not null && !int.TryParse(portValue, NumberStyles.None, CultureInfo.InvariantCulture, out port)
                    ? false
                    : host.Length > 0 && port is > 0 and <= 65535;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        // StackExchange.Redis commonly uses "host:port,password=...".
        var endpoint = candidate.Split(',', 2)[0].Trim();
        if (!Uri.TryCreate($"tcp://{endpoint}", UriKind.Absolute, out var endpointUri))
        {
            return false;
        }

        host = endpointUri.Host;
        port = endpointUri.IsDefaultPort ? defaultPort : endpointUri.Port;
        return host.Length > 0 && port is > 0 and <= 65535;
    }

    private static string? GetValue(DbConnectionStringBuilder builder, string key)
    {
        return builder.TryGetValue(key, out var value)
            ? Convert.ToString(value, CultureInfo.InvariantCulture)
            : null;
    }
}
