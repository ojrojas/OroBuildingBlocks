// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Generic handler for integration events of type <typeparamref name="TIntegrationEvent"/>.
/// </summary>
/// <typeparam name="TIntegrationEvent">The type of integration event to handle.</typeparam>
public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
where TIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// Handles the specified integration event.
    /// </summary>
    /// <param name="integrationEvent">The event to handle.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    Task Handle(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    Task IIntegrationEventHandler.Handle(IntegrationEvent @event, CancellationToken cancellationToken)
    {
        return Handle((TIntegrationEvent)@event, cancellationToken);
    }
}

/// <summary>
/// Non-generic base interface for integration event handlers.
/// </summary>
public interface IIntegrationEventHandler
{
    /// <summary>
    /// Handles the specified integration event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    Task Handle(IntegrationEvent @event, CancellationToken cancellationToken = default);
}
