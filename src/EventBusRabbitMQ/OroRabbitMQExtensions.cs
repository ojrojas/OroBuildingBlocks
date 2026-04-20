namespace OroBuildingBlocks.EventBusRabbitMQ;

public static class OroRabbitMQExtensions
{
    // {
    //   "EventBus": {
    //     "SubscriptionClientName": "...",
    //     "RetryCount": 10
    //   }
    // }

    private const string SectionName = "EventBus";

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
