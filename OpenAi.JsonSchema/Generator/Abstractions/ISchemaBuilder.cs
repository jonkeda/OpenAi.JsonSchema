using OpenAi.JsonSchema.Nodes;


namespace OpenAi.JsonSchema.Generator.Abstractions;

public interface ISchemaBuilder : IValueSchemaBuilder, IObjectSchemaBuilder, IArraySchemaBuilder {
    public SchemaNode BuildSchema(JsonType type, SchemaBuildContext context);
}

public interface IObjectSchemaBuilder {
    public SchemaNode BuildObjectSchema(JsonType type, SchemaBuildContext context);
    public PropertySchema BuildPropertySchema(JsonPropertyType property, SchemaBuildContext context);
}

public interface IArraySchemaBuilder {
    public SchemaNode BuildArraySchema(JsonType type, SchemaBuildContext context);
}

public interface IValueSchemaBuilder {
    public SchemaNode BuildValueSchema(JsonType type, SchemaBuildContext context);
}
