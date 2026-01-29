namespace OroBuildingBlocks.EventBusRabbitMQ;

public class EventBusRabbitMQ(
    ILogger<EventBusRabbitMQ> logger,
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    IOptions<EventBusSubscriptionInfo> eventBusSubscriptionInfo,
    EventBusRabbitMQLogger eventBusRabbitMQLogger) : IEventBus, IDisposable, IHostedService
{
    private const string BROKER_NAME = "eventdrivendesignbus";
    private readonly ResiliencePipeline resiliencePipeline = CreateResiliencePipeline(options.Value.RetryCount);
    private readonly TextMapPropagator textMapPropagator = eventBusRabbitMQLogger.Propagator;
    private readonly ActivitySource activitySource = eventBusRabbitMQLogger.ActivitySource;
    private readonly string queueName = options.Value.SubscriptionClientName;
    private readonly EventBusSubscriptionInfo eventBusSubscriptionInfo = eventBusSubscriptionInfo.Value;
    private IConnection rabbitMQConnection;

    private IChannel channel;

    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<SocketException>(),
            MaxRetryAttempts = retryCount,
            DelayGenerator = (context) => ValueTask.FromResult(GenerateDelay(context.AttemptNumber))
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(retryOptions)
            .Build();

        static TimeSpan? GenerateDelay(int attempt) => TimeSpan.FromSeconds(Math.Pow(2, attempt));
    }

    public async Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Publishing event to RabbitMQ: {EventId}", integrationEvent.Id);
        var nameKey = integrationEvent.GetType().Name;

        if (logger.IsEnabled(LogLevel.Trace))
            logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", integrationEvent.Id, nameKey);

        using var channel = await rabbitMQConnection.CreateChannelAsync(cancellationToken: cancellationToken) ??
        throw new ArgumentNullException("Channel is null");

        if (logger.IsEnabled(LogLevel.Trace))
            logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", integrationEvent.Id);

        await channel.ExchangeDeclareAsync(exchange: BROKER_NAME, type: "direct", cancellationToken: cancellationToken);

        var contentBody = JsonSerializer.SerializeToUtf8Bytes(
            integrationEvent, integrationEvent.GetType(), new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            });

        var activityName = $"{nameKey} Publish";

        await resiliencePipeline.ExecuteAsync(async _ =>
        {
            using var activity = activitySource.StartActivity(activityName, ActivityKind.Producer);
            ActivityContext contextToInject = default;
            if (activity != null)
            {
                contextToInject = activity.Context;
            }
            else if (Activity.Current != null)
            {
                contextToInject = Activity.Current.Context;
            }

            var properties = new BasicProperties()
            {
                DeliveryMode = DeliveryModes.Persistent // persistent
            };

            static void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
            {
                props.Headers ??= new Dictionary<string, object?>();
                props.Headers[key] = value;
            }

            textMapPropagator.Inject(new PropagationContext(contextToInject, Baggage.Current),
                properties,
                InjectTraceContextIntoBasicProperties);

            ArgumentNullException.ThrowIfNull(activity);
            SetActivityContext(activity, nameKey, "publish");

            if (logger.IsEnabled(LogLevel.Trace))
                logger.LogTrace("Publishing event to RabbitMQ: {EventId}", integrationEvent.Id);

            try
            {
                await channel.BasicPublishAsync(
                    exchange: BROKER_NAME,
                    routingKey: nameKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: contentBody, cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                activity.SetExceptionTags(ex);
                throw;
            }
        });
    }

    private static void SetActivityContext(Activity activity, string routingKey, string operation)
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
        channel?.Dispose();
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        static IEnumerable<string> ExtractTraceContextFromBasicProperties(IReadOnlyBasicProperties props, string key)
        {
            if (props.Headers.TryGetValue(key, out var value))
            {
                var bytes = value as byte[];
                return [Encoding.UTF8.GetString(bytes)];
            }
            return [];
        }

        // Extract the PropagationContext of the upstream parent from the message headers.
        var parentContext = textMapPropagator.Extract(default, eventArgs.BasicProperties, ExtractTraceContextFromBasicProperties);
        Baggage.Current = parentContext.Baggage;

        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md
        var activityName = $"{eventArgs.RoutingKey} receive";

        using var activity = activitySource.StartActivity(activityName, ActivityKind.Client, parentContext.ActivityContext);

        SetActivityContext(activity, eventArgs.RoutingKey, "receive");

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

            activity.SetExceptionTags(ex);
        }

        // Even on exception we take the message off the queue.
        // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
        // For more information see: https://www.rabbitmq.com/dlx.html
        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
        }

        await using var scope = serviceProvider.CreateAsyncScope();

        if (!eventBusSubscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        // Deserialize the event
        var integrationEvent = DeserializeMessage(message, eventType);

        // REVIEW: This could be done in parallel

        // Get all the handlers using the event type as the key
        foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
        {
            await handler.Handle(integrationEvent);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private IntegrationEvent DeserializeMessage(string message, Type eventType)
    {
        return JsonSerializer.Deserialize(message, eventType, eventBusSubscriptionInfo.JsonSerializerOptions) as IntegrationEvent;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private byte[] SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), eventBusSubscriptionInfo.JsonSerializerOptions);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Messaging is async so we don't need to wait for it to complete.
        _ = Task.Factory.StartNew(async () =>
        {
            try
            {
                logger.LogInformation("Starting RabbitMQ connection on a background thread");

                rabbitMQConnection = serviceProvider.GetRequiredService<IConnection>();
                if (!rabbitMQConnection.IsOpen)
                {
                    return;
                }

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Creating RabbitMQ consumer channel");
                }

                channel = await rabbitMQConnection.CreateChannelAsync();

                channel.CallbackExceptionAsync += (sender, ea) =>
                {
                    logger.LogWarning(ea.Exception, "Error with RabbitMQ consumer channel");
                    return Task.CompletedTask;
                };

                await channel.ExchangeDeclareAsync(
                    exchange: BROKER_NAME,
                    type: "direct");

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Starting RabbitMQ basic consume");
                }

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += OnMessageReceived;

                await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);

                foreach (var (eventName, _) in eventBusSubscriptionInfo.EventTypes)
                {
                    await channel.QueueBindAsync(
                        queue: queueName,
                        exchange: BROKER_NAME,
                        routingKey: eventName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting RabbitMQ connection");
            }
        },
        TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}