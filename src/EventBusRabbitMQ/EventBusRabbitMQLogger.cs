// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBusRabbitMQ;

/// <summary>
/// Holds shared OpenTelemetry resources (activity source and propagator) for the RabbitMQ event bus.
/// </summary>
public class EventBusRabbitMQLogger
{
    /// <summary>
    /// Gets the activity source name used for OpenTelemetry tracing.
    /// </summary>
    public static string ActivityName { get; } = "OroBuildingBlocks.EventBusRabbitMQ";

    /// <summary>
    /// Gets the <see cref="ActivitySource"/> instance used to start telemetry activities.
    /// </summary>
    public ActivitySource ActivitySource { get; } = new("OroBuildingBlocks.EventBusRabbitMQ");

    /// <summary>
    /// Gets the <see cref="TextMapPropagator"/> used to inject and extract trace context
    /// from RabbitMQ message headers.
    /// </summary>
    public TextMapPropagator Propagator { get; } = Propagators.DefaultTextMapPropagator;
}
