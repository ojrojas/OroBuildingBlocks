namespace BuildingBlocks.ServiceDefaults;

using BuildingBlocks.ServiceDefaults.Observability;

public static class ServiceDefaultsExtensions
{
    /// <summary>
    /// Wires the cross-cutting defaults every service should have:
    /// OpenTelemetry (logs, traces, metrics), health checks and resilient HttpClients.
    /// </summary>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddMeter(OroTradingTelemetry.MeterName)
                .AddMeter(OroTradingTelemetry.MarketDataMeterName)
                .AddMeter(OroTradingTelemetry.ProtectionMeterName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithTracing(tracing => tracing
                .AddSource(builder.Environment.ApplicationName)
                .AddAspNetCoreInstrumentation(options =>
                    // Don't trace health probes.
                    options.Filter = context => !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                                             && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath))
                .AddHttpClientInstrumentation());

        // Export via OTLP when an endpoint is configured (OTEL_EXPORTER_OTLP_ENDPOINT).
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps /health (readiness: all checks) and /alive (liveness: checks tagged "live").
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks(HealthEndpointPath, new HealthCheckOptions
        {
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            ResponseWriter = WriteReadinessResponse
        });
        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live"),
            ResponseWriter = WriteAlivenessResponse
        });

        return app;
    }

    private static Task WriteAlivenessResponse(HttpContext context, HealthReport report)
    {
        return context.Response.WriteAsJsonAsync(new { status = "alive" });
    }

    private static Task WriteReadinessResponse(HttpContext context, HealthReport report)
    {
        var status = report.Status switch
        {
            HealthStatus.Healthy => "ready",
            HealthStatus.Degraded => "degraded",
            _ => "unhealthy"
        };
        var checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString().ToLowerInvariant()
        });

        return context.Response.WriteAsJsonAsync(new { status, checks });
    }
}
