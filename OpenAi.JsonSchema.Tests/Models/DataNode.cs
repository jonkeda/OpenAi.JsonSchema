namespace OpenAi.JsonSchema.Tests.Models;

public record DataNode(
    int Id,
    string Name,
    [property: JsonSchemaIgnore] object Data
);
