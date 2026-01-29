namespace OroBuildingBlocks.EventBus.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent integrationEvent);
        Task IIntegrationEventHandler.Handle(IntegrationEvent @event)
        {
            return Handle((TIntegrationEvent)@event);
        }
    }

    public interface IIntegrationEventHandler
    {
        Task Handle(IntegrationEvent @event);
    }
}