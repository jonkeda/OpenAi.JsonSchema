using System.Text.Json;

namespace OpenAi.JsonSchema.Serialization;

public static class JsonSerializerOptionsExtensions
{
    // Wrap the current resolver so we can modify JsonTypeInfo and still delegate to the original behavior.
    public static JsonSerializerOptions UseAutoInterfacePolymorphism(
        this JsonSerializerOptions options,
        string discriminatorPropertyName = "type")
    {
        var current = options.TypeInfoResolver;
        options.TypeInfoResolver = new AutoPolymorphismJsonTypeInfoResolver(current, discriminatorPropertyName);
        return options;
    }
}