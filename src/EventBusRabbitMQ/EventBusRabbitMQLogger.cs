// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBusRabbitMQ;

public class EventBusRabbitMQLogger
{
    public static string ActivityName = "OroBuildingBlocks.EventBusRabbitMQ";
    public ActivitySource ActivitySource = new("OroBuildingBlocks.EventBusRabbitMQ");
    public TextMapPropagator  Propagator = Propagators.DefaultTextMapPropagator;
}