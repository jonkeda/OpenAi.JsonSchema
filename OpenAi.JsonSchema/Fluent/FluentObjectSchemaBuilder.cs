using OpenAi.JsonSchema.Generator.Abstractions;
using OpenAi.JsonSchema.Nodes;


namespace OpenAi.JsonSchema.Fluent;

public class FluentObjectSchemaBuilder(FluentSchemaBuilder builder, SchemaObjectNode node, JsonType type) : IFluentObjectSchemaBuilder {
    public IFluentObjectSchemaBuilder Description(string description)
    {
        node.Description = description;
        return this;
    }

    public IFluentObjectSchemaBuilder Nullable(bool nullable)
    {
        node.Nullable = nullable;
        return this;
    }

    public IFluentObjectSchemaBuilder Property<TValue>(string property)
    {
        var propertyType = type.CreateProperty(typeof(TValue), property);
        var schema = builder.PropertySchema(propertyType);
        node.AddProperty(schema);
        return this;
    }

    public IFluentObjectSchemaBuilder Property<TValue>(string property, string description)
    {
        var propertyType = type.CreateProperty(typeof(TValue), property);
        var schema = builder.PropertySchema(propertyType);
        schema.Schema.Description = description;
        node.AddProperty(schema);
        return this;
    }

    public IFluentObjectSchemaBuilder Property(string property, string description, Func<IFluentSchemaBuilder, SchemaNode> value)
    {
        var propertyType = type.CreateProperty(typeof(object), property);
        var schema = builder.PropertySchema(propertyType);
        schema.Schema = value(builder);
        schema.Schema.Description = description;
        node.AddProperty(schema);
        return this;
    }

    public IFluentObjectSchemaBuilder Property(string property, Func<IFluentSchemaBuilder, SchemaNode> value)
    {
        var propertyType = type.CreateProperty(typeof(object), property);
        var schema = builder.PropertySchema(propertyType);
        schema.Schema = value(builder);
        node.AddProperty(schema);
        return this;
    }

    public IFluentObjectSchemaBuilder Property<TValue>(string property, string description, TValue value)
    {
        var propertyType = type.CreateProperty(typeof(TValue), property, value);
        var schema = builder.PropertySchema(propertyType);
        schema.Schema.Description = description;
        node.AddProperty(schema);
        return this;
    }
}
