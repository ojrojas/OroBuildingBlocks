// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Abstraction for publishing integration events to a message broker.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to the configured message broker.
    /// </summary>
    /// <param name="integrationEvent">The event to publish.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
