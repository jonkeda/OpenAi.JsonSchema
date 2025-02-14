using OpenAi.JsonSchema.Generator.Abstractions;
using OpenAi.JsonSchema.Nodes;
using System.Linq.Expressions;


namespace OpenAi.JsonSchema.Fluent;

public class FluentObjectSchemaBuilder<T>(FluentSchemaBuilder builder, SchemaObjectNode node, JsonType type) : IFluentObjectSchemaBuilder<T> {
    public IFluentObjectSchemaBuilder<T> Description(string description)
    {
        node.Description = description;
        return this;
    }

    public IFluentObjectSchemaBuilder<T> Nullable(bool nullable)
    {
        node.Nullable = nullable;
        return this;
    }

    public IFluentObjectSchemaBuilder<T> Property<TValue>(Expression<Func<T, TValue>> property)
    {
        var propertyType = GetProperty(property);
        var schema = builder.PropertySchema(propertyType);
        node.AddProperty(schema);
        return this;
    }

    public IFluentObjectSchemaBuilder<T> Property<TValue>(Expression<Func<T, TValue>> property, string description)
    {
        var propertyType = GetProperty(property);
        var schema = builder.PropertySchema(propertyType);
        schema.Schema.Description = description;
        node.AddProperty(schema);
        return this;
    }

    public IFluentObjectSchemaBuilder<T> Property<TValue>(Expression<Func<T, TValue>> property, string description, Func<IFluentSchemaBuilder, SchemaNode> value)
    {
        var propertyType = GetProperty(property);
        var schema = builder.PropertySchema(propertyType);
        schema.Schema = value(builder);
        schema.Schema.Description = description;
        node.AddProperty(schema);
        return this;
    }

    public IFluentObjectSchemaBuilder<T> Property<TValue>(Expression<Func<T, TValue>> property, Func<IFluentSchemaBuilder, SchemaNode> value)
    {
        var propertyType = GetProperty(property);
        var schema = builder.PropertySchema(propertyType);
        schema.Schema = value(builder);
        node.AddProperty(schema);
        return this;
    }

    public IFluentObjectSchemaBuilder<T> Property<TValue>(string property, string description, TValue value)
    {
        var propertyType = type.CreateProperty(typeof(TValue), property, value);
        var schema = builder.PropertySchema(propertyType);
        node.AddProperty(schema);
        return this;
    }

    private JsonPropertyType GetProperty<TReturn>(Expression<Func<T, TReturn>> property)
    {
        var member = MemberName(property);
        var prop = type.Properties.Single(_ => _.MemberName == member);
        return prop;
    }

    private static string MemberName<TReturn>(Expression<Func<T, TReturn>> property)
    {
        if (property.Body is not MemberExpression { Member.Name: { } memberName }) {
            throw new Exception($"Provided Expression is not a MemberExpression: {property.Body}");
        }

        return memberName;
    }
}
