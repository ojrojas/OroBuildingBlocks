namespace BuildingBlocks.EventBus.RabbitMQ;

public interface IRabbitMqConnection : IAsyncDisposable
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Lazily opens and caches a single AMQP connection for the process.
/// The client's automatic recovery keeps it alive across broker restarts.
/// </summary>
public sealed class RabbitMqConnection(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConnection> logger) : IRabbitMqConnection
{
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            var settings = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
                VirtualHost = settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            var attempt = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    if (_connection is not null)
                    {
                        await _connection.DisposeAsync();
                    }

                    _connection = await factory.CreateConnectionAsync(cancellationToken);
                    logger.LogInformation("Connected to RabbitMQ at {HostName}:{Port}", settings.HostName, settings.Port);
                    return _connection;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    // Never give up: a cold/slow broker (Aspire cold start,
                    // production restarts) must not fault the consumer/outbox
                    // BackgroundServices — a throw there stops the Host and
                    // crashes the process (SIGABRT). Backoff capped at 30s.
                    attempt++;
                    var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, attempt), 30));
                    logger.LogWarning(exception,
                        "Could not connect to RabbitMQ (attempt {Attempt}). Retrying in {Delay}s",
                        attempt, delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }
}
