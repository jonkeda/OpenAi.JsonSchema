using System.Text.Json;
using OpenAi.JsonSchema.Fluent;
using OpenAi.JsonSchema.Generator;
using OpenAi.JsonSchema.Nodes;
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
        var options = new JsonSchemaOptions(SchemaDefaults.OpenAi, Helper.JsonOptions);

        var schema = generator.Build(options, _ => _
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
        var options = new JsonSchemaOptions(SchemaDefaults.OpenAi, Helper.JsonOptionsSnakeCase);

        var schema = generator.Build(options, _ => _
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


    [Fact]
    public void Test_Actions()
    {
        var generator = new DefaultSchemaGenerator();
        var options = new JsonSchemaOptions(SchemaDefaults.OpenAi, Helper.JsonOptionsSnakeCase);

        const int numActions = 4;
        var tools = new[] {
            new {
                Name = "Action1",
                Description = "Action number one",
                ArgumentsType = typeof(ExampleArguments1)
            },
            new {
                Name = "Action2",
                Description = "Action number two",
                ArgumentsType = typeof(ExampleArguments2)
            }
        };

        var schema = generator.Build(options, _ => _
            .Object<ResearchActions>(_ => _
                .Property(_ => _.Actions, $"List of {numActions} actions to expand the research", _ => _
                    .Array<ResearchAction>(_ => _
                        .Property(_ => _.Goal, "First talk about the goal of the research that this query is meant to accomplish, " +
                                               "then go deeper into how to advance the research once the results are found, mention additional research directions. " +
                                               "Be as specific as possible, especially for additional research directions.")
                        .Property(_ => _.Action, "Choose the action to take to accomplish the goal and fill out the arguments.", _ => _
                            .AnyOf(
                                tools.Select(Func<IFluentSchemaBuilder, SchemaNode> (tool) => builder => builder
                                    .Object(_ => _
                                        .Description(tool.Description)
                                        .Property("function", _ => _.Const(tool.Name))
                                        .Property("arguments", _ => _.Value(tool.ArgumentsType))
                                    )
                                ).ToArray()
                            )
                        )
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
    public void Test_array_anyof()
    {
        var generator = new DefaultSchemaGenerator();
        var options = new JsonSchemaOptions(SchemaDefaults.OpenAi, Helper.JsonOptionsSnakeCase);

        var schema = generator.Build(options, _ => _
            .Object(_ => _
                .Property("answer", _ => _
                    .Array(_ => _
                        .AnyOf(
                            _ => _.Object("Write markdown text.", _ => _
                                .Property("$type", _ => _.Const("text"))
                                .Property("content", _ => _.Value<string>())
                            ),
                            _ => _.Object("Show a visually appearing widget.", _ => _
                                .Property("$type", _ => _.Const("widget"))
                                .Property("id", "Value form widgetId in context.", _ => _.Value<string>())
                                .Property("comment", "A user-facing `comment`", _ => _.Value<string>())
                            ),
                            _ => _.Object("Add a reference to your statement.", _ => _
                                .Property("$type", _ => _.Const("citation"))
                                .Property("title", _ => _.Value<string>())
                                .Property("url", "URL from context.", _ => _.Value<string>())
                            )
                        )
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
    public void Test_dynamic_enum()
    {
        var generator = new DefaultSchemaGenerator();
        var options = new JsonSchemaOptions(SchemaDefaults.OpenAi, Helper.JsonOptionsSnakeCase);

        var schema = generator.Build(options, _ => _
            .Object(_ => _
                .Property("answer", _ => _
                    .Array(_ => _
                        .AnyOf(
                            _ => _.Object("Write markdown text.", _ => _
                                .Property("$type", _ => _.Const("text"))
                                .Property("content", _ => _.Value<string>())
                            ),
                            _ => _.Object("Show a visually appearing widget.", _ => _
                                .Property("$type", _ => _.Const("widget"))
                                .Property("id", "Value form widgetId in context.", _ => _.Enum(["widget1", "widget2", "widget3"]))
                                .Property("comment", "A user-facing `comment`", _ => _.Value<string>())
                            ),
                            _ => _.Object("Add a reference to your statement.", _ => _
                                .Property("$type", _ => _.Const("citation"))
                                .Property("title", _ => _.Value<string>())
                                .Property("url", "URL from context.", _ => _.Value<string>())
                            )
                        )
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

internal record ResearchActions(
    ResearchAction[] Actions
);

internal record ResearchAction(
    string Goal,
    JsonElement Action
);

internal record ExampleArguments1(string Query);

internal record ExampleArguments2(string DocumentName);
