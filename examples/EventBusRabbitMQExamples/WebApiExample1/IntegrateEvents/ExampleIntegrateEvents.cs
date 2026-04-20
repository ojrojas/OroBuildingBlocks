using OroBuildingBlocks.EventBus.Events;

namespace WebApiExample1.IntegrateEvents;

public record ExampleIntegrateEvents(string Message): IntegrationEvent;
