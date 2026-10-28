using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.Kernel.Domain.Entities;
using BuildingBlocks.Kernel.Domain.Enumerations;
using BuildingBlocks.Kernel.Domain.Events;
using BuildingBlocks.Kernel.Domain.Rules;
using BuildingBlocks.Kernel.Domain.Specifications;
using BuildingBlocks.Kernel.Domain.ValueObjects;
using BuildingBlocks.EventBus.Abstractions;

namespace BuildingBlocks.Tests.Helpers;

public sealed class TestEntity(Guid id) : Entity<Guid>(id)
{
    public static void ExposeCheckRule(IBusinessRule rule) => CheckRule(rule);
}

public sealed class TestAggregateRoot(Guid id) : AggregateRoot<Guid>(id)
{
    public void ExposeRaiseDomainEvent(IDomainEvent domainEvent) => RaiseDomainEvent(domainEvent);
}

public sealed class TestValueObject(string name, int age) : ValueObject
{
    public string Name { get; } = name;
    public int Age { get; } = age;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return Age;
    }
}

public sealed record OrderId(Guid Value) : StronglyTypedId<Guid>(Value);

public sealed class TestEnumeration : Enumeration<TestEnumeration>
{
    public static readonly TestEnumeration One = new(1, "One");
    public static readonly TestEnumeration Two = new(2, "Two");
    public static readonly TestEnumeration Three = new(3, "Three");

    public TestEnumeration(int id, string name) : base(id, name)
    {
    }
}

public sealed record TestDomainEvent : DomainEvent;

public sealed class AlwaysBrokenRule : IBusinessRule
{
    public bool IsBroken() => true;
    public string Message => "Always broken";
}

public sealed class NeverBrokenRule : IBusinessRule
{
    public bool IsBroken() => false;
    public string Message => "Never broken";
}

public sealed class TestOrder
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class ActiveOrdersSpecification : Specification<TestOrder>
{
    public ActiveOrdersSpecification() => Where(o => o.IsActive);
}

public sealed class CheapOrdersSpecification : Specification<TestOrder>
{
    public CheapOrdersSpecification(decimal maxPrice) => Where(o => o.Price <= maxPrice);
}

public sealed class EmptySpecification<T> : Specification<T>
{
}

public sealed class TestRequest : IRequest<string>;
public sealed class TestCommand : ICommand<string>;
public sealed class TestQuery : IQuery<string>;

public sealed class TestRequestHandler : IRequestHandler<TestRequest, string>
{
    public Task<string> HandleAsync(TestRequest request, CancellationToken cancellationToken)
        => Task.FromResult("handled");
}

public sealed class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<string> HandleAsync(TestCommand request, CancellationToken cancellationToken)
        => Task.FromResult("command-handled");
}

public sealed class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public Task<string> HandleAsync(TestQuery request, CancellationToken cancellationToken)
        => Task.FromResult("query-handled");
}

public sealed record TestIntegrationEvent : IntegrationEvent;

public sealed class TestIntegrationEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
{
    public Task HandleAsync(TestIntegrationEvent integrationEvent, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
