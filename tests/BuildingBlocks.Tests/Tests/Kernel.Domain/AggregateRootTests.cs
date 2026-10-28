using BuildingBlocks.Tests.Helpers;
using BuildingBlocks.Kernel.Domain.Events;

namespace BuildingBlocks.Tests.Kernel.Domain;

public class AggregateRootTests
{
    [Fact]
    public void DomainEvents_initially_empty()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        Assert.NotNull(aggregate.DomainEvents);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_adds_event_to_collection()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();

        aggregate.ExposeRaiseDomainEvent(domainEvent);

        Assert.Single(aggregate.DomainEvents);
        Assert.Same(domainEvent, aggregate.DomainEvents.First());
    }

    [Fact]
    public void RaiseDomainEvent_multiple_events_accumulate()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        aggregate.ExposeRaiseDomainEvent(event1);
        aggregate.ExposeRaiseDomainEvent(event2);

        Assert.Equal(2, aggregate.DomainEvents.Count);
    }

    [Fact]
    public void ClearDomainEvents_clears_all_events()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.ExposeRaiseDomainEvent(new TestDomainEvent());
        aggregate.ExposeRaiseDomainEvent(new TestDomainEvent());

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_returns_readonly_collection()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        var events = aggregate.DomainEvents;

        Assert.IsAssignableFrom<IReadOnlyCollection<IDomainEvent>>(events);
    }

    [Fact]
    public void DomainEvents_cannot_be_cast_to_list()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.ExposeRaiseDomainEvent(new TestDomainEvent());

        var events = aggregate.DomainEvents;

        Assert.Throws<InvalidCastException>(() =>
        {
            var list = (List<IDomainEvent>)events;
            list.Add(new TestDomainEvent());
        });
    }
}
