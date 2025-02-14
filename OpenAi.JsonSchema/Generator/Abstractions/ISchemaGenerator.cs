using System.Text.Json;
using OpenAi.JsonSchema.Fluent;
using OpenAi.JsonSchema.Nodes;
using OpenAi.JsonSchema.Serialization;


namespace OpenAi.JsonSchema.Generator.Abstractions;

public interface ISchemaGenerator {
    // static
    public SchemaRootNode Generate(Type type, JsonSerializerOptions? options = null);
    public SchemaRootNode Generate(Type type, JsonSchemaOptions? options = null);
    public SchemaRootNode Generate<T>(JsonSerializerOptions? options = null) => Generate(typeof(T), options);
    public SchemaRootNode Generate<T>(JsonSchemaOptions? options = null) => Generate(typeof(T), options);

    // fluent
    public SchemaRootNode Build(Func<IFluentSchemaBuilder, SchemaNode> builder);
    public SchemaRootNode Build(JsonSerializerOptions options, Func<IFluentSchemaBuilder, SchemaNode> builder);
    public SchemaRootNode Build(JsonSchemaOptions options, Func<IFluentSchemaBuilder, SchemaNode> builder);
}
