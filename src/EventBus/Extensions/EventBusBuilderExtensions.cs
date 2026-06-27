// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBus.Extensions;

/// <summary>
/// Extension methods for <see cref="IEventBusBuilder"/> to configure JSON options and register event handlers.
/// </summary>
public static class EventBusBuilderExtensions
{
    /// <summary>
    /// Configures the JSON serializer options used for event serialization and deserialization.
    /// </summary>
    /// <param name="builder">The event bus builder.</param>
    /// <param name="configure">A delegate to customize the <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>The same builder instance for chaining.</returns>
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

    /// <summary>
    /// Registers an integration event and its corresponding handler.
    /// </summary>
    /// <typeparam name="T">The integration event type.</typeparam>
    /// <typeparam name="TH">The handler type that implements <see cref="IIntegrationEventHandler{T}"/>.</typeparam>
    /// <param name="builder">The event bus builder.</param>
    /// <returns>The same builder instance for chaining.</returns>
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
