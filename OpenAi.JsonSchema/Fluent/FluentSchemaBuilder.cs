using OpenAi.JsonSchema.Generator.Abstractions;
using OpenAi.JsonSchema.Nodes;


namespace OpenAi.JsonSchema.Fluent;

public class FluentSchemaBuilder(ISchemaBuilder builder, SchemaBuildContext context) : IFluentSchemaBuilder {
    internal SchemaNode Schema(JsonType jsonType) => builder.BuildSchema(jsonType, context);
    internal PropertySchema PropertySchema(JsonPropertyType jsonType) => builder.BuildPropertySchema(jsonType, context);

    internal (SchemaNode node, SchemaObjectNode objectNode) ObjectSchema(JsonType type)
    {
        var node = builder.BuildObjectSchema(type, context);

        var objectNode = node as SchemaObjectNode ?? (
            context.Definitions.TryGetRef(type.Type, out var @ref)
                ? @ref.Value as SchemaObjectNode
                : null
        ) ?? throw new Exception("Failed to resolve SchemaObjectNode!");

        return (node, objectNode);
    }

    internal JsonType Resolve(Type type) => context.Resolver.GetTypeInfo(type);
    internal JsonType Resolve(Type type, object? value) => context.Resolver.GetTypeInfo(type, null, value);


    public SchemaNode Object(string description, Action<IFluentObjectSchemaBuilder> properties)
    {
        var type = Resolve(typeof(object));
        var node = new SchemaObjectNode([], [], false) {
            Description = description,
        };
        properties(new FluentObjectSchemaBuilder(this, node, type));
        return node;
    }

    public SchemaNode Object(Action<IFluentObjectSchemaBuilder> properties)
    {
        var type = Resolve(typeof(object));
        var node = new SchemaObjectNode([], [], false);
        properties(new FluentObjectSchemaBuilder(this, node, type));
        return node;
    }

    public SchemaNode Object<T>() where T : class
    {
        return Schema(Resolve(typeof(T)));
    }

    public SchemaNode Object<T>(string description) where T : class
    {
        var schema = Schema(Resolve(typeof(T)));
        schema.Description = description;
        return schema;
    }

    public SchemaNode Object<T>(string description, Action<IFluentObjectSchemaBuilder<T>> properties) where T : class
    {
        var type = Resolve(typeof(T));
        var (node, obj) = ObjectSchema(type);
        obj.Properties.Clear();
        obj.Required.Clear();
        obj.Description = description;
        properties(new FluentObjectSchemaBuilder<T>(this, obj, type));
        return node;
    }

    public SchemaNode Object<T>(Action<IFluentObjectSchemaBuilder<T>> properties) where T : class
    {
        var type = Resolve(typeof(T));
        var (node, obj) = ObjectSchema(type);
        obj.Properties.Clear();
        obj.Required.Clear();
        properties(new FluentObjectSchemaBuilder<T>(this, obj, type));
        return node;
    }

    public SchemaNode Array<T>()
    {
        return Schema(Resolve(typeof(T)));
    }

    public SchemaNode Array<T>(string description)
    {
        var items = Schema(Resolve(typeof(T)));
        return new SchemaArrayNode(items) {
            Description = description
        };
    }

    public SchemaNode Array<T>(string description, Action<IFluentObjectSchemaBuilder<T>> properties) where T : class
    {
        var items = Object(properties);
        return new SchemaArrayNode(items) {
            Description = description
        };
    }

    public SchemaNode Array<T>(Action<IFluentObjectSchemaBuilder<T>> properties) where T : class
    {
        return new SchemaArrayNode(Object(properties));
    }

    public SchemaNode AnyOf(params Func<IFluentSchemaBuilder, SchemaNode>[] values)
    {
        var options = values.Select(action => action(this)).ToArray();
        return new SchemaAnyOfNode(options);
    }

    public SchemaNode AnyOf(string description, params Func<IFluentSchemaBuilder, SchemaNode>[] values)
    {
        var options = values.Select(action => action(this)).ToArray();
        return new SchemaAnyOfNode(options) {
            Description = description
        };
    }

    public SchemaNode Const<T>(T value)
    {
        var type = Resolve(typeof(T), value);
        return Schema(type);
    }
}
