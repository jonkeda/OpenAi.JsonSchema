namespace OpenAi.JsonSchema.Tests.Models;

public record DataNode(
    int Id,
    string Name,
    [property: JsonSchemaIgnore] object Data
);

public record NullalbeTypes(
    int? Id,
    string? Name,
    Status? Status,
    int[]? Numbers,
    DataNode? Object
);
