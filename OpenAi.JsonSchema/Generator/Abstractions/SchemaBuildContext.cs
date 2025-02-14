using OpenAi.JsonSchema.Serialization;


namespace OpenAi.JsonSchema.Generator.Abstractions;

public class SchemaBuildContext(JsonTypeResolver resolver, JsonSchemaOptions options) {
    public DefinitionCollection Definitions { get; } = new();
    public JsonTypeResolver Resolver { get; } = resolver;
    public JsonSchemaOptions Options { get; } = options;
}
