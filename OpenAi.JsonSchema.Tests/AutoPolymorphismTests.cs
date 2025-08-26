using System.Text.Json;
using OpenAi.JsonSchema.Generator;
using OpenAi.JsonSchema.Serialization;
using OpenAi.JsonSchema.Tests.Models;
using Xunit.Abstractions;

namespace OpenAi.JsonSchema.Tests;

public class AutoPolymorphismTests(ITestOutputHelper output)
{
    public interface ITestShape
    {
        string Name { get; }
    }

    public sealed class TestCircle : ITestShape
    {
        public string Name => "Circle";
        public double Radius { get; set; }
    }

    public sealed class TestRectangle : ITestShape
    {
        public string Name => "Rectangle";
        public double Width { get; set; }
        public double Height { get; set; }
    }

    [Fact]
    public void Test_AutoPolymorphismJsonTypeInfoResolver()
    {
        // Arrange: Configure JsonSerializerOptions with auto-polymorphism using Default schema (not OpenAI)
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .UseAutoInterfacePolymorphism("type");
        
        var options = new JsonSchemaOptions(SchemaDefaults.Default, jsonOptions);

        // Act: Generate schema for interface
        var generator = new DefaultSchemaGenerator();
        var schema = generator.Generate(typeof(ITestShape), options);

        // Assert: Verify schema generation succeeds
        var json = schema.ToJson();
        output.WriteLine(json);
        Assert.NotNull(json);
        
        // Verify the schema contains polymorphism information
        Assert.Contains("anyOf", json);
        Assert.Contains("TestCircle", json);
        Assert.Contains("TestRectangle", json);
        
        Helper.Assert(json);
    }

    [Fact]
    public void Test_AutoPolymorphismJsonTypeInfoResolver_WithConcreteType()
    {
        // Arrange: Configure JsonSerializerOptions with auto-polymorphism
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .UseAutoInterfacePolymorphism("type");
        
        var options = new JsonSchemaOptions(SchemaDefaults.OpenAi, jsonOptions);

        // Act: Generate schema for concrete type (should not be affected by auto-polymorphism)
        var generator = new DefaultSchemaGenerator();
        var schema = generator.Generate(typeof(TestCircle), options);

        // Assert: Verify schema generation succeeds for concrete types
        var json = schema.ToJson();
        output.WriteLine(json);
        Assert.NotNull(json);
        
        // Concrete types should not have polymorphism added automatically
        Assert.DoesNotContain("anyOf", json);
        // Property names are camelCase due to JsonSerializerDefaults.Web
        Assert.Contains("radius", json);
        
        Helper.Assert(json);
    }
}