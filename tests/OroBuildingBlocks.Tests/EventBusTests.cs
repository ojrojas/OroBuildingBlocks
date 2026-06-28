namespace OroBuildingBlocks.Tests;

public class EventBusTests
{
    [Fact]
    public void IntegrationEvent_DefaultConstructor_SetsIdAndCreated()
    {
        var @event = new TestIntegrationEvent();

        Assert.NotEqual(Guid.Empty, @event.Id);
        Assert.NotEqual(default, @event.Created);
    }

    [Fact]
    public void IntegrationEvent_ParameterizedConstructor_SetsIdAndCreated()
    {
        var id = Guid.NewGuid();
        DateTime created = DateTime.UtcNow;
        var @event = new TestIntegrationEvent(id, created);

        Assert.Equal(id, @event.Id);
        Assert.Equal(created, @event.Created);
    }

    private record TestIntegrationEvent : EventBus.Events.IntegrationEvent
    {
        public TestIntegrationEvent() { }
        public TestIntegrationEvent(Guid id, DateTime created) : base(id, created) { }
    }
}
