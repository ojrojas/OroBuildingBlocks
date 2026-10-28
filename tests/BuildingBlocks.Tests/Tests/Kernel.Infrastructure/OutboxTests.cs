using BuildingBlocks.Kernel.Infrastructure.Outbox;

namespace BuildingBlocks.Tests.Kernel.Infrastructure;

public class OutboxMessageTests
{
    [Fact]
    public void EventType_defaults_to_empty_string()
    {
        var message = new OutboxMessage();

        Assert.Equal(string.Empty, message.EventType);
    }

    [Fact]
    public void Payload_defaults_to_empty_string()
    {
        var message = new OutboxMessage();

        Assert.Equal(string.Empty, message.Payload);
    }

    [Fact]
    public void ProcessedOnUtc_is_nullable()
    {
        var message = new OutboxMessage();

        Assert.Null(message.ProcessedOnUtc);
    }

    [Fact]
    public void Error_is_nullable()
    {
        var message = new OutboxMessage();

        Assert.Null(message.Error);
    }

    [Fact]
    public void Properties_can_be_set()
    {
        var now = DateTime.UtcNow;
        var message = new OutboxMessage
        {
            EventType = "TestEvent",
            Payload = "{\"key\":\"value\"}",
            OccurredOnUtc = now,
            ProcessedOnUtc = now,
            Error = "some error"
        };

        Assert.Equal("TestEvent", message.EventType);
        Assert.Equal("{\"key\":\"value\"}", message.Payload);
        Assert.Equal(now, message.OccurredOnUtc);
        Assert.Equal(now, message.ProcessedOnUtc);
        Assert.Equal("some error", message.Error);
    }
}

public class OutboxProcessorOptionsTests
{
    [Fact]
    public void Default_PollingInterval_is_5_seconds()
    {
        var options = new OutboxOptions();

        Assert.Equal(TimeSpan.FromSeconds(5), options.PollingInterval);
    }

    [Fact]
    public void Default_BatchSize_is_50()
    {
        var options = new OutboxOptions();

        Assert.Equal(50, options.BatchSize);
    }

    [Fact]
    public void SectionName_is_correct()
    {
        Assert.Equal("Outbox", OutboxOptions.SectionName);
    }
}
