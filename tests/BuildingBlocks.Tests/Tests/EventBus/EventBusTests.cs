using BuildingBlocks.EventBus.Abstractions;
using BuildingBlocks.EventBus.Subscriptions;
using BuildingBlocks.Tests.Helpers;

namespace BuildingBlocks.Tests.EventBus;

public class IntegrationEventTests
{
    [Fact]
    public void Id_is_generated_on_creation()
    {
        var @event = new TestIntegrationEvent();

        Assert.NotEqual(Guid.Empty, @event.Id);
    }

    [Fact]
    public void Id_is_unique_per_instance()
    {
        var a = new TestIntegrationEvent();
        var b = new TestIntegrationEvent();

        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void OccurredOnUtc_is_recent()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var @event = new TestIntegrationEvent();
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(@event.OccurredOnUtc, before, after);
    }

    [Fact]
    public void Implements_record()
    {
        var @event = new TestIntegrationEvent();

        Assert.IsAssignableFrom<IntegrationEvent>(@event);
    }
}

public class EventBusSubscriptionManagerTests
{
    [Fact]
    public void AddSubscription_creates_record()
    {
        var manager = new EventBusSubscriptionManager();

        manager.AddSubscription<TestIntegrationEvent, TestIntegrationEventHandler>();

        Assert.Single(manager.Subscriptions);
    }

    [Fact]
    public void AddSubscription_same_event_adds_handler()
    {
        var manager = new EventBusSubscriptionManager();

        manager.AddSubscription<TestIntegrationEvent, TestIntegrationEventHandler>();
        manager.AddSubscription<TestIntegrationEvent, AnotherHandler>();

        Assert.Single(manager.Subscriptions);
        Assert.Equal(2, manager.Subscriptions.First().HandlerTypes.Count);
    }

    [Fact]
    public void TryGetSubscription_returns_true_for_known_event()
    {
        var manager = new EventBusSubscriptionManager();
        manager.AddSubscription<TestIntegrationEvent, TestIntegrationEventHandler>();

        var found = manager.TryGetSubscription(
            EventBusSubscriptionManager.GetEventName<TestIntegrationEvent>(),
            out var subscription);

        Assert.True(found);
        Assert.NotNull(subscription);
        Assert.Equal(typeof(TestIntegrationEvent), subscription.EventType);
    }

    [Fact]
    public void TryGetSubscription_returns_false_for_unknown_event()
    {
        var manager = new EventBusSubscriptionManager();

        var found = manager.TryGetSubscription("UnknownEvent", out var subscription);

        Assert.False(found);
        Assert.Null(subscription);
    }

    [Fact]
    public void GetEventName_returns_type_name()
    {
        var name = EventBusSubscriptionManager.GetEventName<TestIntegrationEvent>();

        Assert.Equal("TestIntegrationEvent", name);
    }

    [Fact]
    public void GetEventName_returns_type_name_from_type()
    {
        var name = EventBusSubscriptionManager.GetEventName(typeof(TestIntegrationEvent));

        Assert.Equal("TestIntegrationEvent", name);
    }

    [Fact]
    public void Subscriptions_initially_empty()
    {
        var manager = new EventBusSubscriptionManager();

        Assert.Empty(manager.Subscriptions);
    }

    private sealed class AnotherHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        public Task HandleAsync(TestIntegrationEvent integrationEvent, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}

public class SubscriptionInfoTests
{
    [Fact]
    public void Fields_are_set_correctly()
    {
        var info = new SubscriptionInfo(
            "TestEvent",
            typeof(TestIntegrationEvent),
            [typeof(TestIntegrationEventHandler)]);

        Assert.Equal("TestEvent", info.EventName);
        Assert.Equal(typeof(TestIntegrationEvent), info.EventType);
        Assert.Single(info.HandlerTypes);
    }

    [Fact]
    public void HandlerTypes_is_mutable()
    {
        var info = new SubscriptionInfo(
            "TestEvent",
            typeof(TestIntegrationEvent),
            [typeof(TestIntegrationEventHandler)]);

        info.HandlerTypes.Add(typeof(AnotherHandlerForTests));

        Assert.Equal(2, info.HandlerTypes.Count);
    }

    private sealed class AnotherHandlerForTests : IIntegrationEventHandler<TestIntegrationEvent>
    {
        public Task HandleAsync(TestIntegrationEvent integrationEvent, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
