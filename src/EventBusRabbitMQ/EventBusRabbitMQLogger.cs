namespace OroBuildingBlocks.EventBusRabbitMQ;

public class EventBusRabbitMQLogger
{
    public string ActivityName = "OroBuildingBlocks.EventBusRabbitMQ";
    public ActivitySource ActivitySource = new("OroBuildingBlocks.EventBusRabbitMQ");
    public TextMapPropagator  Propagator = Propagators.DefaultTextMapPropagator;
}