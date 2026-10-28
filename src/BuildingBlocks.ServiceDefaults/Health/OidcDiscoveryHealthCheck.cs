namespace BuildingBlocks.ServiceDefaults.Health;

/// <summary>Verifies that the configured OIDC authority publishes discovery metadata.</summary>
public sealed class OidcDiscoveryHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var authority = configuration["Identity:Authority"];
        if (!Uri.TryCreate(authority, UriKind.Absolute, out var authorityUri) ||
            authorityUri.Scheme is not ("https" or "http"))
        {
            return HealthCheckResult.Unhealthy("Identity authority is not configured.");
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            var discoveryUri = new Uri($"{authorityUri.ToString().TrimEnd('/')}/.well-known/openid-configuration");
            using var response = await httpClientFactory.CreateClient(nameof(OidcDiscoveryHealthCheck))
                .GetAsync(discoveryUri, linked.Token)
                ;

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Identity discovery endpoint is unavailable.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException or UriFormatException)
        {
            return HealthCheckResult.Unhealthy("Identity discovery endpoint is unavailable.");
        }
    }
}
