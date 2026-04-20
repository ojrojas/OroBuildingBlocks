// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBusRabbitMQ;
public class EventBusOptions
{
    public string SubscriptionClientName { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 5;
}