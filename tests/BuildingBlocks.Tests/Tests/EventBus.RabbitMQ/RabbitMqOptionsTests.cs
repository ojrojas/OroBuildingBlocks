using BuildingBlocks.EventBus.RabbitMQ;

namespace BuildingBlocks.Tests.EventBus.RabbitMQ;

public class RabbitMqOptionsTests
{
    [Fact]
    public void Default_HostName_is_localhost()
    {
        var options = new RabbitMqOptions();

        Assert.Equal("localhost", options.HostName);
    }

    [Fact]
    public void Default_Port_is_5672()
    {
        var options = new RabbitMqOptions();

        Assert.Equal(5672, options.Port);
    }

    [Fact]
    public void Default_UserName_is_guest()
    {
        var options = new RabbitMqOptions();

        Assert.Equal("guest", options.UserName);
    }

    [Fact]
    public void Default_Password_is_guest()
    {
        var options = new RabbitMqOptions();

        Assert.Equal("guest", options.Password);
    }

    [Fact]
    public void Default_VirtualHost_is_slash()
    {
        var options = new RabbitMqOptions();

        Assert.Equal("/", options.VirtualHost);
    }

    [Fact]
    public void Default_ExchangeName_is_integration_events()
    {
        var options = new RabbitMqOptions();

        Assert.Equal("integration_events", options.ExchangeName);
    }

    [Fact]
    public void Default_QueueName_is_empty()
    {
        var options = new RabbitMqOptions();

        Assert.Equal(string.Empty, options.QueueName);
    }

    [Fact]
    public void Default_RetryCount_is_5()
    {
        var options = new RabbitMqOptions();

        Assert.Equal(5, options.RetryCount);
    }

    [Fact]
    public void Default_PrefetchCount_is_10()
    {
        var options = new RabbitMqOptions();

        Assert.Equal(10, options.PrefetchCount);
    }

    [Fact]
    public void SectionName_is_correct()
    {
        Assert.Equal("EventBus:RabbitMq", RabbitMqOptions.SectionName);
    }
}
