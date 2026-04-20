// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBus.Abstractions;

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
