namespace OroBuildingBlocks.EventBus.Abstractions;

public interface IEventBusBuilder
{
    IServiceCollection Services { get; }
}