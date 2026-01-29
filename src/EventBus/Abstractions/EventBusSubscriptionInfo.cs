using System.Text.Json.Serialization.Metadata;

namespace OroBuildingBlocks.EventBus.Abstractions;


public class EventBusSubscriptionInfo
{
   public Dictionary<string, Type> EventTypes { get; } = [];
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