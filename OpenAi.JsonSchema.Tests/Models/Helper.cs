using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OpenAi.JsonSchema.Generator;
using OpenAi.JsonSchema.Nodes;
using OpenAi.JsonSchema.Serialization;


namespace OpenAi.JsonSchema.Tests.Models;

public static class Helper {
    public static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true,
    };

    public static readonly JsonSerializerOptions JsonOptionsCamelCase = new(JsonSerializerDefaults.Web) {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static readonly JsonSerializerOptions JsonOptionsSnakeCase = new(JsonSerializerDefaults.Web) {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    public static SchemaRootNode Generate<T>(JsonSchemaOptions? options = null)
    {
        return Generate(typeof(T), options);
    }

    public static SchemaRootNode Generate(Type type, JsonSchemaOptions? options = null)
    {
        options ??= new JsonSchemaOptions(JsonOptions);

        var resolver = new DefaultSchemaGenerator();
        var schema = resolver.Generate(type, options);

        return schema;
    }

    public static JsonNode GenerateJsonNode(Type type)
    {
        return Generate(type).ToJsonNode();
    }

    public static void Assert(string actual, [CallerFilePath] string path = null!, [CallerMemberName] string name = null!)
    {
        var fileName = path.Split("OpenAi.JsonSchema.Tests")[1].TrimStart('\\', '/');
        var outFileName = Path.Combine(AppContext.BaseDirectory, "../../../Output/", $"{Path.ChangeExtension(fileName, null)}.{name}.json");
        if (File.Exists(outFileName)) {
            var expected = File.ReadAllText(outFileName);
            Xunit.Assert.Equal(expected, actual);
        }
        else {
            Directory.CreateDirectory(Path.GetDirectoryName(outFileName)!);
            File.WriteAllText(outFileName, actual);
            Xunit.Assert.Fail("Could not verify output as the file was not there.\n - The file is now created with current output!");
        }
    }
}
