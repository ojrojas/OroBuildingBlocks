// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBusRabbitMQ;

/// <summary>
/// Extension methods for registering the RabbitMQ-backed event bus with the application's DI container.
/// </summary>
public static class OroRabbitMQExtensions
{
    // {
    //   "EventBus": {
    //     "SubscriptionClientName": "...",
    //     "RetryCount": 10
    //   }
    // }

    private const string SectionName = "EventBus";

    /// <summary>
    /// Registers the RabbitMQ event bus implementation (<see cref="EventBusRabbitMQ"/>) and related services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="connectionName">The connection name used to resolve the RabbitMQ connection string from configuration.</param>
    /// <returns>An <see cref="IEventBusBuilder"/> for further configuration.</returns>
    public static IEventBusBuilder AddRabbitMqEventBus(this IHostApplicationBuilder builder, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddRabbitMQClient(connectionName);

        // RabbitMQ.Client doesn't have built-in support for OpenTelemetry, so we need to add it ourselves
        builder.Services.AddOpenTelemetry()
           .WithTracing(tracing =>
           {
               tracing.AddSource(EventBusRabbitMQLogger.ActivityName);
           });

        // Options support
        builder.Services.Configure<EventBusOptions>(builder.Configuration.GetSection(SectionName));
        builder.Services.AddSingleton<IValidateOptions<EventBusOptions>, EventBusOptionsValidator>();

        // Abstractions on top of the core client API
        builder.Services.AddSingleton<EventBusRabbitMQLogger>();
        builder.Services.AddSingleton<IEventBus, EventBusRabbitMQ>();
        // Start consuming messages as soon as the application starts
        builder.Services.AddSingleton<IHostedService>(sp => (EventBusRabbitMQ)sp.GetRequiredService<IEventBus>());

        return new EventBusBuilder(builder.Services);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}
