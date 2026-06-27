// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBus.Events;

/// <summary>
/// Base record for all integration events sent through the event bus.
/// Provides a unique identifier and creation timestamp.
/// </summary>
public record IntegrationEvent
{
    /// <summary>
    /// Creates a new event with a random <see cref="Guid"/> and the current UTC timestamp.
    /// </summary>
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        Created = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new event with the specified identifier and creation timestamp.
    /// </summary>
    /// <param name="id">The unique event identifier.</param>
    /// <param name="created">The event creation timestamp in UTC.</param>
    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime created)
    {
        Id = id;
        Created = created;
    }

    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    [JsonInclude]
    public Guid Id { get; private init; }

    /// <summary>
    /// Gets the UTC timestamp when this event was created.
    /// </summary>
    [JsonInclude]
    public DateTime Created { get; private init; }
}
