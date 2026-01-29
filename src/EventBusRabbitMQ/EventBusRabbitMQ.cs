namespace OroBuildingBlocks.EventBusRabbitMQ;

public sealed class EventBusRabbitMQ(
    ILogger<EventBusRabbitMQ> logger,
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    IOptions<EventBusSubscriptionInfo> subscriptionInfo,
    EventBusRabbitMQLogger telemetry) : IEventBus, IDisposable, IHostedService
{
    private const string ExchangeName = "eventdrivendesignbus";
    
    private readonly ResiliencePipeline _resiliencePipeline = CreateResiliencePipeline(options.Value.RetryCount);
    private readonly string _queueName = options.Value.SubscriptionClientName;
    private readonly EventBusSubscriptionInfo _subscriptionInfo = subscriptionInfo.Value;
    
    private IConnection? _connection;
    private IChannel? _consumerChannel;

    public async Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var eventName = integrationEvent.GetType().Name;
        
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Publishing event to RabbitMQ: {EventId} ({EventName})", integrationEvent.Id, eventName);
        }

        _connection ??= serviceProvider.GetRequiredService<IConnection>();
        
        using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        
        await channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct, cancellationToken: cancellationToken);

        var body = SerializeMessage(integrationEvent);
        var activityName = $"{eventName} Publish";

        await _resiliencePipeline.ExecuteAsync(async ct =>
        {
            using var activity = telemetry.ActivitySource.StartActivity(activityName, ActivityKind.Producer);
            
            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            InjectTelemetryContext(activity, properties);
            SetActivityTags(activity, eventName, "publish");

            try
            {
                await channel.BasicPublishAsync(
                    exchange: ExchangeName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: ct
                );
            }
            catch (Exception ex)
            {
                activity?.SetExceptionTags(ex);
                throw;
            }
        }, cancellationToken);
    }

    private void InjectTelemetryContext(Activity? activity, BasicProperties properties)
    {
        var contextToInject = activity?.Context ?? Activity.Current?.Context ?? default;
        
        telemetry.Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current),
            properties,
            (props, key, value) =>
            {
                props.Headers ??= new Dictionary<string, object?>();
                props.Headers[key] = value;
            });
    }

    private static void SetActivityTags(Activity? activity, string routingKey, string operation)
    {
        if (activity is not null)
        {
            activity.SetTag("messaging.system", "rabbitmq");
            activity.SetTag("messaging.destination_kind", "queue");
            activity.SetTag("messaging.operation", operation);
            activity.SetTag("messaging.destination.name", routingKey);
            activity.SetTag("messaging.rabbitmq.routing_key", routingKey);
        }
    }

    public void Dispose()
    {
        _consumerChannel?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Messaging is async so we don't need to wait for it to complete.
        _ = Task.Run(async () =>
        {
            try
            {
                logger.LogInformation("Starting RabbitMQ connection on a background thread");

                _connection = serviceProvider.GetRequiredService<IConnection>();
                
                if (!_connection.IsOpen)
                {
                    logger.LogWarning("RabbitMQ connection is not open. Consumer will not start.");
                    return;
                }

                _consumerChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                _consumerChannel.CallbackExceptionAsync += (sender, ea) =>
                {
                    logger.LogWarning(ea.Exception, "Error with RabbitMQ consumer channel");
                    return Task.CompletedTask;
                };

                await _consumerChannel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct, cancellationToken: cancellationToken);

                await _consumerChannel.QueueDeclareAsync(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
                consumer.ReceivedAsync += OnMessageReceived;

                await _consumerChannel.BasicConsumeAsync(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken);

                foreach (var (eventName, _) in _subscriptionInfo.EventTypes)
                {
                    await _consumerChannel.QueueBindAsync(
                        queue: _queueName,
                        exchange: ExchangeName,
                        routingKey: eventName,
                        cancellationToken: cancellationToken);
                }
                
                logger.LogInformation("RabbitMQ consumer started for queue {QueueName}", _queueName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting RabbitMQ connection");
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        var parentContext = telemetry.Propagator.Extract(default, eventArgs.BasicProperties, (props, key) =>
        {
            if (props.Headers?.TryGetValue(key, out var value) == true && value is byte[] bytes)
            {
                return [Encoding.UTF8.GetString(bytes)];
            }
            return [];
        });

        Baggage.Current = parentContext.Baggage;

        var activityName = $"{eventArgs.RoutingKey} receive";
        using var activity = telemetry.ActivitySource.StartActivity(activityName, ActivityKind.Client, parentContext.ActivityContext);

        SetActivityTags(activity, eventArgs.RoutingKey, "receive");

        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            activity?.SetTag("message", message);

            if (message.Contains("throw-fake-exception", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
            }

            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error Processing message \"{Message}\"", message);
            activity?.SetExceptionTags(ex);
        }
        finally
        {
            if (_consumerChannel != null)
            {
                await _consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            }
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
        }

        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        var integrationEvent = DeserializeMessage(message, eventType);
        if (integrationEvent == null)
        {
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        
        var handlers = scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType);
        
        foreach (var handler in handlers)
        {
            await handler.Handle(integrationEvent);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "Serialization is handled via options.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Serialization is handled via options.")]
    private IntegrationEvent? DeserializeMessage(string message, Type eventType)
    {
        return JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as IntegrationEvent;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "Serialization is handled via options.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Serialization is handled via options.")]
    private byte[] SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
    }

    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<SocketException>(),
                MaxRetryAttempts = retryCount,
                DelayGenerator = context => ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(Math.Pow(2, context.AttemptNumber)))
            })
            .Build();
    }
}