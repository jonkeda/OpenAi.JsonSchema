using System.Text.Json;
using OpenAi.JsonSchema.Fluent;
using OpenAi.JsonSchema.Generator.Abstractions;
using OpenAi.JsonSchema.Nodes;
using OpenAi.JsonSchema.Serialization;
using OpenAi.JsonSchema.Validation;


namespace OpenAi.JsonSchema.Generator;

public class DefaultSchemaGenerator(ISchemaBuilder? builder = null) : ISchemaGenerator {
    internal ISchemaBuilder Builder { get; } = builder ?? new DefaultSchemaBuilder();

    public virtual SchemaRootNode Generate(Type type, JsonSerializerOptions? options = null)
    {
        return Generate(type, new JsonSchemaOptions(options));
    }

    public virtual SchemaRootNode Generate(Type type, JsonSchemaOptions? options = null)
    {
        options ??= JsonSchemaOptions.Default;

        var resolver = new JsonTypeResolver(options.JsonSerializerOptions);
        var context = new SchemaBuildContext(resolver, options);

        var info = resolver.GetTypeInfo(type);
        var schema = Builder.BuildSchema(info, context);

        return CreateRootNode(schema, context);
    }

    public SchemaRootNode Generate<T>(JsonSerializerOptions? options = null) => Generate(typeof(T), options);
    public SchemaRootNode Generate<T>(JsonSchemaOptions? options = null) => Generate(typeof(T), options);


    public SchemaRootNode Build(Func<IFluentSchemaBuilder, SchemaNode> builder)
    {
        var options = JsonSchemaOptions.Default;

        var resolver = new JsonTypeResolver(options.JsonSerializerOptions);
        var context = new SchemaBuildContext(resolver, options);

        var fluent = new FluentSchemaBuilder(Builder, context);
        var schema = builder(fluent);

        return CreateRootNode(schema, context);
    }


    public SchemaRootNode Build(JsonSerializerOptions options, Func<IFluentSchemaBuilder, SchemaNode> builder) => Build(new JsonSchemaOptions(options), builder);

    public SchemaRootNode Build(JsonSchemaOptions options, Func<IFluentSchemaBuilder, SchemaNode> builder)
    {
        var resolver = new JsonTypeResolver(options.JsonSerializerOptions);
        var context = new SchemaBuildContext(resolver, options);

        var fluent = new FluentSchemaBuilder(Builder, context);
        var schema = builder(fluent);

        return CreateRootNode(schema, context);
    }


    private static SchemaRootNode CreateRootNode(SchemaNode schema, SchemaBuildContext context)
    {
        var options = context.Options;

        var definitions = new SchemaDefinitionNode(context.Definitions.Values);
        var root = new SchemaRootNode(schema, definitions, options);

        if (options.Transformer is { } transformer) {
            root = transformer.Transform(root) as SchemaRootNode ?? throw new Exception("Schema Transformer must return a SchemaRootNode!");
        }

        if (options.Validator is { } validator) {
            var result = validator.Validate(root);
            if (!result.Valid) throw new ValidationException(result.Errors);
        }

        return root;
    }
}
