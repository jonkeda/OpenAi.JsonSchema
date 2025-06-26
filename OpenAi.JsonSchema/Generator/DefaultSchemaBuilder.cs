using OpenAi.JsonSchema.Generator.Abstractions;
using OpenAi.JsonSchema.Nodes;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace OpenAi.JsonSchema.Generator;

public class DefaultSchemaBuilder() : ISchemaBuilder {
    public virtual SchemaNode BuildSchema(JsonType type, SchemaBuildContext context)
    {
        return type.Kind switch {
            JsonTypeKind.Object => BuildObjectSchema(type, context),
            JsonTypeKind.Array => BuildArraySchema(type, context),
            JsonTypeKind.Value => BuildValueSchema(type, context),
            _ => throw new ArgumentOutOfRangeException(nameof(type.Kind), type.Kind, null)
        };
    }


    #region Object

    public virtual SchemaNode BuildObjectSchema(JsonType type, SchemaBuildContext context)
    {
        if (type.PolymorphismOptions is { } options) {
            var schemas = options.Select(type => BuildObjectSchemaCached(type, context)).ToArray();
            return new SchemaAnyOfNode(schemas);
        }

        return BuildObjectSchemaCached(type, context);
    }

    protected virtual SchemaNode BuildObjectSchemaCached(JsonType type, SchemaBuildContext context)
    {
        if (!context.Definitions.TryGetRef(type.Type, out var @ref)) {
            @ref = BuildObjectRefSchema(type, context);
        }

        var refNode = new SchemaRefNode(@ref);

        if (type.Nullable is true) {
            return new SchemaAnyOfNode(new SchemaValueNode("null"), refNode);
        }

        return refNode;
    }

    protected virtual SchemaRefValue BuildObjectRefSchema(JsonType type, SchemaBuildContext context)
    {
        var schema = new SchemaObjectNode([], [], false);

        var @ref = context.Definitions.CreateRef(type.Type, schema);

        if (type.Type.GetCustomAttribute(typeof(DescriptionAttribute), inherit: false) is DescriptionAttribute description) {
            schema.Description = description.Description;
        }

        if (type is { TypeDiscriminatorName: { } name, TypeDiscriminatorValue: { } value }) {
            var property = type.CreateProperty(value.GetType(), name, value);
            var propertySchema = BuildPropertySchema(property, context);
            schema.AddProperty(propertySchema);
        }

        foreach (var property in type.Properties) {
            var propertySchema = BuildPropertySchema(property, context);
            schema.AddProperty(propertySchema);
        }

        return @ref;
    }

    public virtual PropertySchema BuildPropertySchema(JsonPropertyType property, SchemaBuildContext context)
    {
        var schema = BuildSchema(property.PropertyType, context);

        var required = (context.Options.PropertyRequired ?? PropertyRequired).Invoke(property, context);

        var descriptionAttribute = property.Attributes.GetCustomAttributes(typeof(DescriptionAttribute), false).OfType<DescriptionAttribute>().FirstOrDefault();
        if (descriptionAttribute is not null) {
            schema.Description = descriptionAttribute.Description;
        }

        return new PropertySchema(
            Name: property.Name,
            Schema: schema,
            Required: required
        );
    }

    protected virtual bool PropertyRequired(JsonPropertyType property, SchemaBuildContext context)
    {
        var nullable = property.PropertyType.Nullable is true;
        return property.IsRequired || !nullable;
    }

    #endregion


    #region Array

    public virtual SchemaNode BuildArraySchema(JsonType type, SchemaBuildContext context)
    {
        var elementType = type.ElementType ?? type.GenericTypeArguments[0];

        var schema = new SchemaArrayNode(
            Items: BuildSchema(elementType, context),
            Nullable: type.Nullable is true
        );

        if (type.Type.GetCustomAttribute(typeof(DescriptionAttribute), inherit: false) is DescriptionAttribute description) {
            schema.Description = description.Description;
        }

        return schema;
    }

    #endregion


    #region Value

    public virtual SchemaNode BuildValueSchema(JsonType info, SchemaBuildContext context)
    {
        var type = info.Type;
        if (
            type == typeof(JsonElement) ||
            type == typeof(JsonNode) ||
            type == typeof(object)
        ) {
            return new SchemaAnyNode();
        }

        var node = BuildValueNode(info, context);

        if (info.Value is { } value) {
            return new SchemaConstNode(node.Type, value);
        }

        return node;
    }

    protected virtual SchemaValueNode BuildValueNode(JsonType info, SchemaBuildContext context)
    {
        var type = info.Type;
        var nullable = info.Nullable is true;

        if (type == typeof(string) || type == typeof(char)) {
            return new SchemaValueNode("string", nullable);
        }
        else if (type == typeof(int) ||
                 type == typeof(long) ||
                 type == typeof(uint) ||
                 type == typeof(byte) ||
                 type == typeof(sbyte) ||
                 type == typeof(ulong) ||
                 type == typeof(short) ||
                 type == typeof(ushort)) {
            return new SchemaValueNode("integer", nullable);
        }
        else if (type == typeof(float) ||
                 type == typeof(double) ||
                 type == typeof(decimal)) {
            return new SchemaValueNode("number", nullable);
        }
        else if (type == typeof(bool)) {
            return new SchemaValueNode("boolean", nullable);
        }
        else if (type == typeof(DateTime) || type == typeof(DateTimeOffset)) {
            return context.Options.FormatSupported
                ? new SchemaFormatNode("string", "date-time", nullable)
                : new SchemaValueNode("string", nullable) { Description = "date-time" };
        }
        else if (type == typeof(DateOnly)) {
            return context.Options.FormatSupported
                ? new SchemaFormatNode("string", "date", nullable)
                : new SchemaValueNode("string", nullable) { Description = "date" };
        }
        else if (type == typeof(TimeOnly)) {
            return context.Options.FormatSupported
                ? new SchemaFormatNode("string", "time", nullable)
                : new SchemaValueNode("string", nullable) { Description = "time" };
        }
        else if (type == typeof(Guid)) {
            return context.Options.FormatSupported
                ? new SchemaFormatNode("string", "uuid", nullable)
                : new SchemaValueNode("string", nullable) { Description = "uuid" };
        }
        else if (type.IsEnum) {
            return SchemaEnumNode.Create(type, nullable, context.Options.JsonSerializerOptions);
        }
        else if (GetDefault(type) is IFormattable) {
            return new SchemaValueNode("string", nullable);
        }
        else {
            throw new ArgumentOutOfRangeException(nameof(type), type.FullName, null);
        }
    }

    private static object? GetDefault(Type type) => Array.CreateInstance(type, 1).GetValue(0);

    #endregion
}
