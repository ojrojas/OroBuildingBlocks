// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Holds runtime subscription information including registered event types and JSON serialization options.
/// </summary>
public class EventBusSubscriptionInfo
{
    /// <summary>
    /// Gets a dictionary that maps event type names to their CLR types.
    /// </summary>
    public Dictionary<string, Type> EventTypes { get; } = new Dictionary<string, Type>();

    /// <summary>
    /// Gets the JSON serializer options used for deserializing incoming events.
    /// Defaults to camelCase naming, ignoring nulls, and supporting all Unicode ranges.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
    };

    internal static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions
    {
        TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault ? CreateDefaultTypeResolver() : JsonTypeInfoResolver.Combine(),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
    };

    private static IJsonTypeInfoResolver CreateDefaultTypeResolver()
    {
        var resolvers = new List<IJsonTypeInfoResolver>
        {
            JsonTypeInfoResolver.Combine(),
            new DefaultJsonTypeInfoResolver()
        };

        return JsonTypeInfoResolver.Combine(resolvers.ToArray());
    }
}
