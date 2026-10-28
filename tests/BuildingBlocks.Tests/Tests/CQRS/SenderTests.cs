using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.CQRS.Behaviors;
using BuildingBlocks.CQRS.Dispatching;
using BuildingBlocks.CQRS.Validation;
using BuildingBlocks.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

namespace BuildingBlocks.Tests.CQRS;

public class SenderTests
{
    [Fact]
    public async Task SendAsync_throws_when_request_is_null()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var sender = new Sender(provider);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sender.SendAsync<string>(null!, CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_resolves_handler_and_returns_result()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        var result = await sender.SendAsync<string>(new TestRequest(), CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task SendAsync_executes_pipeline_behaviors()
    {
        var callOrder = new List<string>();

        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddScoped<IPipelineBehavior<TestRequest, string>>(sp =>
            new TrackingBehavior("behavior1", callOrder));
        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        var result = await sender.SendAsync<string>(new TestRequest(), CancellationToken.None);

        Assert.Equal("handled", result);
        Assert.Contains("behavior1", callOrder);
    }

    [Fact]
    public async Task SendAsync_executes_multiple_behaviors_in_order()
    {
        var callOrder = new List<string>();

        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddScoped<IPipelineBehavior<TestRequest, string>>(sp =>
            new TrackingBehavior("first", callOrder));
        services.AddScoped<IPipelineBehavior<TestRequest, string>>(sp =>
            new TrackingBehavior("second", callOrder));
        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        var result = await sender.SendAsync<string>(new TestRequest(), CancellationToken.None);

        Assert.Equal("handled", result);
        var firstIndex = callOrder.IndexOf("first");
        var secondIndex = callOrder.IndexOf("second");
        Assert.True(firstIndex < secondIndex, $"Expected 'first' before 'second', but got: [{string.Join(", ", callOrder)}]");
    }

    private sealed class TrackingBehavior(string name, List<string> callOrder)
        : IPipelineBehavior<TestRequest, string>
    {
        public async Task<string> HandleAsync(
            TestRequest request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            callOrder.Add(name);
            return await next(cancellationToken);
        }
    }
}

public class DomainEventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_with_no_events_does_nothing()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var logger = Substitute.For<ILogger<DomainEventDispatcher>>();
        var dispatcher = new DomainEventDispatcher(provider, logger);

        await dispatcher.DispatchAsync([], CancellationToken.None);
    }

    [Fact]
    public async Task DispatchAsync_invokes_registered_handler()
    {
        var handler = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        var services = new ServiceCollection();
        services.AddSingleton(handler);
        var provider = services.BuildServiceProvider();
        var logger = Substitute.For<ILogger<DomainEventDispatcher>>();
        var dispatcher = new DomainEventDispatcher(provider, logger);

        var domainEvent = new TestDomainEvent();
        await dispatcher.DispatchAsync([domainEvent], CancellationToken.None);

        await handler.Received(1).HandleAsync(domainEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_invokes_multiple_handlers_for_same_event()
    {
        var handler1 = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        var handler2 = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        var services = new ServiceCollection();
        services.AddSingleton(handler1);
        services.AddSingleton(handler2);
        var provider = services.BuildServiceProvider();
        var logger = Substitute.For<ILogger<DomainEventDispatcher>>();
        var dispatcher = new DomainEventDispatcher(provider, logger);

        var domainEvent = new TestDomainEvent();
        await dispatcher.DispatchAsync([domainEvent], CancellationToken.None);

        await handler1.Received(1).HandleAsync(domainEvent, Arg.Any<CancellationToken>());
        await handler2.Received(1).HandleAsync(domainEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_with_multiple_events_dispatches_all()
    {
        var handler = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        var services = new ServiceCollection();
        services.AddSingleton(handler);
        var provider = services.BuildServiceProvider();
        var logger = Substitute.For<ILogger<DomainEventDispatcher>>();
        var dispatcher = new DomainEventDispatcher(provider, logger);

        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        await dispatcher.DispatchAsync([event1, event2], CancellationToken.None);

        await handler.Received(1).HandleAsync(event1, Arg.Any<CancellationToken>());
        await handler.Received(1).HandleAsync(event2, Arg.Any<CancellationToken>());
    }
}

public class ValidationBehaviorTests
{
    [Fact]
    public async Task HandleAsync_with_no_validators_calls_next()
    {
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var nextCalled = false;

        var result = await behavior.HandleAsync(
            new TestRequest(),
            _ => { nextCalled = true; return Task.FromResult("result"); },
            CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("result", result);
    }

    [Fact]
    public async Task HandleAsync_with_valid_validator_calls_next()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<TestRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ValidationFailure>>([]));
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var result = await behavior.HandleAsync(
            new TestRequest(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task HandleAsync_with_failing_validator_throws()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        var failures = new List<ValidationFailure> { new("Prop", "Error") };
        validator.ValidateAsync(Arg.Any<TestRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ValidationFailure>>(failures));
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.HandleAsync(
                new TestRequest(),
                _ => Task.FromResult("ok"),
                CancellationToken.None));

        Assert.Single(ex.Failures);
        Assert.Equal("Prop", ex.Failures.First().PropertyName);
    }

    [Fact]
    public async Task HandleAsync_accumulates_failures_from_multiple_validators()
    {
        var validator1 = Substitute.For<IValidator<TestRequest>>();
        validator1.ValidateAsync(Arg.Any<TestRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ValidationFailure>>(
                [new ValidationFailure("P1", "E1")]));

        var validator2 = Substitute.For<IValidator<TestRequest>>();
        validator2.ValidateAsync(Arg.Any<TestRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ValidationFailure>>(
                [new ValidationFailure("P2", "E2")]));

        var behavior = new ValidationBehavior<TestRequest, string>([validator1, validator2]);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.HandleAsync(
                new TestRequest(),
                _ => Task.FromResult("ok"),
                CancellationToken.None));

        Assert.Equal(2, ex.Failures.Count);
    }
}

public class LoggingBehaviorTests
{
    [Fact]
    public async Task HandleAsync_calls_next_and_returns_response()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);

        var result = await behavior.HandleAsync(
            new TestRequest(),
            _ => Task.FromResult("logged"),
            CancellationToken.None);

        Assert.Equal("logged", result);
    }

    [Fact]
    public async Task HandleAsync_logs_information_on_success()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);

        await behavior.HandleAsync(
            new TestRequest(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        logger.Received(2).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("TestRequest")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task HandleAsync_logs_error_and_rethrows_on_exception()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.HandleAsync(
                new TestRequest(),
                _ => throw new InvalidOperationException("fail"),
                CancellationToken.None));

        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<InvalidOperationException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
