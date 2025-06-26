using System.Text.Json.Serialization;


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

public record ConstModeTest(Element Element);

[JsonDerivedType(typeof(ElementText), "text")]
[JsonDerivedType(typeof(ElementImage), "image")]
public record Element();

public record ElementText(string Text) : Element;

public record ElementImage(string Src) : Element;
