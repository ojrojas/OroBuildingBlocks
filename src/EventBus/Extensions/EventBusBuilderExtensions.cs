using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using OroBuildingBlocks.EventBus.Abstractions;
using OroBuildingBlocks.EventBus.Events;

namespace OroBuildingBlocks.EventBus.Extensions;

public static class EventBusBuilderExtensions
{
    public static IEventBusBuilder ConfigureJsonOptions(this IEventBusBuilder builder, Action<JsonSerializerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.Configure<EventBusSubscriptionInfo>(opt =>
        {
            configure(opt.JsonSerializerOptions);
        });

        return builder;
    }

    public static IEventBusBuilder AddSubscriptionManager<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TH>(this IEventBusBuilder builder)
        where T : IntegrationEvent
        where TH : class, IIntegrationEventHandler<T>
    {
        builder.Services.AddKeyedTransient<IIntegrationEventHandler, TH>(typeof(T));
        builder.Services.Configure<EventBusSubscriptionInfo>(opt =>
        {
            opt.EventTypes[typeof(T).Name] = typeof(T);
        });

        return builder;
    }
}