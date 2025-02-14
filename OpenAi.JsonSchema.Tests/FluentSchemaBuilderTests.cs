using System.Text.Json;
using OpenAi.JsonSchema.Generator;
using OpenAi.JsonSchema.Serialization;
using OpenAi.JsonSchema.Tests.Models;
using Xunit.Abstractions;


namespace OpenAi.JsonSchema.Tests;

public class FluentSchemaBuilderTests(ITestOutputHelper output) {
    [Fact]
    public void Test_Default()
    {
        var generator = new DefaultSchemaGenerator();

        var schema = generator.Build(Helper.JsonOptions, _ => _
            .Object<FluentDocument>("A document", _ => _
                .Property(_ => _.Id, "Id of the document")
                .Property(_ => _.Name, "Document Name")
                .Property(_ => _.Lines, "Text lines of the document", _ => _
                    .Array<Line>(_ => _
                        .Description("A line of text in a document")
                        .Property(_ => _.Number, "Line Number")
                        .Property(_ => _.Text, "Line text")
                    )
                )
                .Property(_ => _.Next, "Next document in order")
                .Property(_ => _.Prev, "Prev document in order")
                .Property(_ => _.Metadata, "Some Metadata", _ => _
                    .Object(_ => _
                        .Property("type", _ => _.Const("meta"))
                        .Property<string>("author", "Author of the document")
                        .Property<DateTime>("published", "published date")
                    )
                )
                .Property(_ => _.Extra, "Some Extra", _ => _
                    .AnyOf(
                        _ => _.Object<Person>(),
                        _ => _.Object<Organization>()
                    )
                )
            )
        );

        var json = schema.ToJson();
        output.WriteLine(json);
        Assert.NotNull(json);
        Helper.Assert(json);
    }


    [Fact]
    public void Test_OpenAi()
    {
        var generator = new DefaultSchemaGenerator();

        var schema = generator.Build(new JsonSchemaOptions(SchemaDefaults.OpenAi, Helper.JsonOptions), _ => _
            .Object<FluentDocument>("A document", _ => _
                .Property(_ => _.Id, "Id of the document")
                .Property(_ => _.Name, "Document Name")
                .Property(_ => _.Lines, "Text lines of the document", _ => _
                    .Array<Line>(_ => _
                        .Description("A line of text in a document")
                        .Property(_ => _.Number, "Line Number")
                        .Property(_ => _.Text, "Line text")
                    )
                )
                .Property(_ => _.Next, "Next document in order")
                .Property(_ => _.Prev, "Prev document in order")
                .Property(_ => _.Metadata, "Some Metadata", _ => _
                    .Object(_ => _
                        .Property("type", _ => _.Const("meta"))
                        .Property<string>("author", "Author of the document")
                        .Property<DateTime>("published", "published date")
                    )
                )
                .Property(_ => _.Extra, "Some Extra", _ => _
                    .AnyOf(
                        _ => _.Object<Person>(),
                        _ => _.Object<Organization>()
                    )
                )
            )
        );

        var json = schema.ToJson();
        output.WriteLine(json);
        Assert.NotNull(json);
        Helper.Assert(json);
    }


    [Fact]
    public void Test_SnakeCase()
    {
        var generator = new DefaultSchemaGenerator();

        var schema = generator.Build(new JsonSchemaOptions(SchemaDefaults.OpenAi, Helper.JsonOptionsSnakeCase), _ => _
            .Object("A person", _ => _
                .Property<string>("fullName", "Firstname and Lastname")
                .Property("metaData", "Some Metadata", _ => _
                    .Object(_ => _
                        .Property("type", _ => _.Const("meta"))
                        .Property<string>("author", "Author of the document")
                        .Property<DateTime>("published", "published date")
                    )
                )
                .Property("extra", "Some Extra", _ => _
                    .AnyOf(
                        _ => _.Object<Person>(),
                        _ => _.Object<Organization>()
                    )
                )
            )
        );

        var json = schema.ToJson();
        output.WriteLine(json);
        Assert.NotNull(json);
        Helper.Assert(json);
    }
}

public record FluentDocument(
    int Id,
    string Name,
    Line[] Lines,
    FluentDocument? Next,
    FluentDocument? Prev,
    JsonElement Metadata,
    JsonElement Extra
);

public record Line(int Number, string Text);

public record Person(string Name, int Age);

public record Organization(string Name, int Address);
