using System.Diagnostics.CodeAnalysis;

namespace OroBuildingBlocks.EventBus.Extensions;

public static class  EventBusBuilderExtensions
{
    extension(IEventBusBuilder builder)
    {
        // Extension methods for EventBusBuilder can be added here in the future
        public IEventBusBuilder ConfigureJsonOptions(Action<JsonSerializerOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            builder.Services.Configure<EventBusSubscriptionInfo>(opt =>
            {
                configure(opt.JsonSerializerOptions);
            });

            return builder;
        }

        public IEventBusBuilder AddSubscriptionManager<T, [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors)] TH>(
    )
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
}