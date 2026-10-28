using BuildingBlocks.EventBus.Abstractions;

namespace WebApiExample1.IntegrateEvents;

public record ExampleIntegrateEvents(string Message): IntegrationEvent;
