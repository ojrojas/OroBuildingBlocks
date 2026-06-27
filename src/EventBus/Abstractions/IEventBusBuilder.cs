// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Builder interface for configuring the event bus services.
/// </summary>
public interface IEventBusBuilder
{
    /// <summary>
    /// Gets the service collection used to register event bus dependencies.
    /// </summary>
    IServiceCollection Services { get; }
}
