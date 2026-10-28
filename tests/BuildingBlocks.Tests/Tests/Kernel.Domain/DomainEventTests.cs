using BuildingBlocks.Tests.Helpers;
using BuildingBlocks.Kernel.Domain.Events;
using BuildingBlocks.Kernel.Domain.Exceptions;
using BuildingBlocks.Kernel.Domain.Rules;

namespace BuildingBlocks.Tests.Kernel.Domain;

public class DomainEventTests
{
    [Fact]
    public void EventId_is_not_empty()
    {
        var domainEvent = new TestDomainEvent();

        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
    }

    [Fact]
    public void EventId_is_unique_per_instance()
    {
        var a = new TestDomainEvent();
        var b = new TestDomainEvent();

        Assert.NotEqual(a.EventId, b.EventId);
    }

    [Fact]
    public void OccurredOnUtc_is_recent()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var domainEvent = new TestDomainEvent();
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void Implements_IDomainEvent()
    {
        var domainEvent = new TestDomainEvent();

        Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
    }
}

public class BusinessRuleValidationExceptionTests
{
    [Fact]
    public void Contains_broken_rule()
    {
        var rule = new AlwaysBrokenRule();

        var ex = new BusinessRuleValidationException(rule);

        Assert.Same(rule, ex.BrokenRule);
    }

    [Fact]
    public void Message_matches_rule_message()
    {
        var rule = new AlwaysBrokenRule();

        var ex = new BusinessRuleValidationException(rule);

        Assert.Equal("Always broken", ex.Message);
    }

    [Fact]
    public void Inherits_from_DomainException()
    {
        var rule = new AlwaysBrokenRule();

        var ex = new BusinessRuleValidationException(rule);

        Assert.IsAssignableFrom<DomainException>(ex);
    }
}

public class DomainExceptionTests
{
    [Fact]
    public void Message_is_set()
    {
        var ex = new DomainException("test message");

        Assert.Equal("test message", ex.Message);
    }

    [Fact]
    public void Inherits_from_Exception()
    {
        var ex = new DomainException("test");

        Assert.IsAssignableFrom<Exception>(ex);
    }
}
