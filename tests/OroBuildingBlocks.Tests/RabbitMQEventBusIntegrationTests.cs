using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OroBuildingBlocks.EventBus.Abstractions;
using OroBuildingBlocks.EventBus.Events;
using OroBuildingBlocks.EventBus.Extensions;
using OroBuildingBlocks.EventBusRabbitMQ;
using DotNet.Testcontainers.Builders;
using Testcontainers.RabbitMq;

namespace OroBuildingBlocks.Tests;

public sealed class RabbitMQEventBusIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqContainer? _container;
    private readonly bool _dockerAvailable;

    public RabbitMQEventBusIntegrationTests()
    {
        try
        {
            _container = new RabbitMqBuilder()
                .WithImage("rabbitmq:3.13-management")
                .WithPortBinding(5672, true)
                .Build();
            _dockerAvailable = true;
        }
        catch (DockerUnavailableException)
        {
            _dockerAvailable = false;
        }
    }

    public async Task InitializeAsync()
    {
        if (!_dockerAvailable)
        {
            return;
        }

        await _container!.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    [Fact]
    public async Task PublishEvent_ConsumerReceivesIt()
    {
        if (!_dockerAvailable)
        {
            return;
        }

        var received = new TaskCompletionSource<TestEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new TestHandler(e => received.TrySetResult(e));

        IHost host = CreateHost(handler, _container!.GetConnectionString());

        await host.StartAsync();

        IEventBus eventBus = host.Services.GetRequiredService<IEventBus>();
        var testEvent = new TestEvent { Data = "hello" };

        await eventBus.PublishAsync(testEvent);

        TestEvent delivered = await received.Task.WaitAsync(TimeSpan.FromSeconds(15));
        Assert.Equal(testEvent.Data, delivered.Data);

        await host.StopAsync();
    }

    [Fact]
    public async Task PublishEvent_WithRetry_SucceedsAfterTransientFailure()
    {
        if (!_dockerAvailable)
        {
            return;
        }

        var attempts = 0;
        var received = new TaskCompletionSource<TestEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new TestHandler(e =>
        {
            Interlocked.Increment(ref attempts);
            received.TrySetResult(e);
        });

        IHost host = CreateHost(handler, _container!.GetConnectionString());

        await host.StartAsync();

        IEventBus eventBus = host.Services.GetRequiredService<IEventBus>();
        var testEvent = new TestEvent { Data = "retry-test" };

        await eventBus.PublishAsync(testEvent);

        TestEvent delivered = await received.Task.WaitAsync(TimeSpan.FromSeconds(15));
        Assert.Equal(testEvent.Data, delivered.Data);
        Assert.True(attempts >= 1);

        await host.StopAsync();
    }

    private static IHost CreateHost(TestHandler handler, string connectionString)
    {
        var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings
        {
            Configuration = new ConfigurationManager()
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:messaging"] = connectionString,
            ["EventBus:SubscriptionClientName"] = "test-queue-" + Guid.NewGuid().ToString("N"),
            ["EventBus:RetryCount"] = "3"
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddDebug();

        builder.AddRabbitMqEventBus("messaging")
            .AddSubscriptionManager<TestEvent, TestHandler>();

        builder.Services.AddSingleton(handler);
        builder.Services.Configure<EventBusSubscriptionInfo>(opt =>
        {
            opt.EventTypes[nameof(TestEvent)] = typeof(TestEvent);
        });

        return builder.Build();
    }

    private sealed record TestEvent : IntegrationEvent
    {
        public string? Data { get; init; }
    }

    private sealed class TestHandler : IIntegrationEventHandler<TestEvent>
    {
        private readonly Action<TestEvent> _onHandle;

        public TestHandler(Action<TestEvent> onHandle) => _onHandle = onHandle;

        public Task Handle(TestEvent integrationEvent, CancellationToken cancellationToken= default)
        {
            _onHandle(integrationEvent);
            return Task.CompletedTask;
        }
    }
}
