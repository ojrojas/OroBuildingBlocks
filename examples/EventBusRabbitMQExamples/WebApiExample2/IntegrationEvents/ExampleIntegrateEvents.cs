using OroBuildingBlocks.EventBus.Events;

namespace WebApiExample2.IntegrationEvents;

public record ExampleIntegrateEvents: IntegrationEvent
{
    public string Message { get; init; }

    public ExampleIntegrateEvents(string message)
        => Message = message;
}