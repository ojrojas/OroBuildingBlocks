// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBusRabbitMQ;

/// <summary>
/// Configuration options for the RabbitMQ event bus implementation.
/// </summary>
public class EventBusOptions
{
    /// <summary>
    /// Gets or sets the subscription client / queue name.
    /// This value must be configured and non-empty.
    /// </summary>
    public string SubscriptionClientName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of publish retry attempts.
    /// Must be between 0 and 100 inclusive. Defaults to 5.
    /// </summary>
    public int RetryCount { get; set; } = 5;
}

/// <summary>
/// Validates <see cref="EventBusOptions"/> at startup.
/// </summary>
internal sealed class EventBusOptionsValidator : IValidateOptions<EventBusOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, EventBusOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.SubscriptionClientName))
        {
            failures.Add("EventBus:SubscriptionClientName must be configured and non-empty.");
        }

        if (options.RetryCount < 0)
        {
            failures.Add("EventBus:RetryCount must be greater than or equal to 0.");
        }

        if (options.RetryCount > 100)
        {
            failures.Add("EventBus:RetryCount must not exceed 100.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
