using System.Text.Json;
using OpenAi.JsonSchema.Serialization;
using Xunit;

namespace OpenAi.JsonSchema.Tests.Serialization;

public class AutoPolymorphismJsonTypeInfoResolverTests
{
    // Test types must live in the same assembly as the interface to match the resolver's behavior.
    public interface IAnimal { }
    public sealed class Dog : IAnimal { }
    public sealed class Cat : IAnimal { }

    [Fact]
    public void PopulatesPolymorphismOptions_for_interface_from_same_assembly()
    {
        // Arrange
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .UseAutoInterfacePolymorphism("type");

        // Act
        var info = options.TypeInfoResolver.GetTypeInfo(typeof(IAnimal), options);

        // Assert
        Assert.NotNull(info);
        Assert.NotNull(info!.PolymorphismOptions);
        Assert.Equal("type", info.PolymorphismOptions!.TypeDiscriminatorPropertyName);

        var derived = info.PolymorphismOptions.DerivedTypes;
        Assert.Contains(derived, d => d.DerivedType == typeof(Dog) && (string?)d.TypeDiscriminator == "Dog");
        Assert.Contains(derived, d => d.DerivedType == typeof(Cat) && (string?)d.TypeDiscriminator == "Cat");
    }
}