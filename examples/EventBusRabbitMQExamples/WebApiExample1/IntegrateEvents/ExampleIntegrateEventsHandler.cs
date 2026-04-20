using OroBuildingBlocks.EventBus.Abstractions;

namespace WebApiExample1.IntegrateEvents;

public class ExampleIntegrateEventsHandler(ILogger<ExampleIntegrateEventsHandler> logger)
: IIntegrationEventHandler<ExampleIntegrateEvents>
{
    public async Task Handle(ExampleIntegrateEvents integrationEvent)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - {IntegrationEventMessage}",
            integrationEvent.Id, integrationEvent.Message);
        await Task.CompletedTask;
    }
}
