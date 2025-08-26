# Automatic interface polymorphism (opt-in)

If you want interfaces to be handled automatically without adding `[JsonDerivedType]` attributes, enable an opt-in resolver that scans only the interface’s assembly and registers all concrete implementations. The discriminator will be emitted as a property named `type` by default.

```csharp
using System.Text.Json;
using OpenAi.JsonSchema.Serialization;

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    .UseAutoInterfacePolymorphism("type");

// Example interface and implementations live in the same assembly:
public interface IShape { }
public sealed class Circle : IShape { public double Radius { get; set; } }
public sealed class Rectangle : IShape { public double Width { get; set; } public double Height { get; set; } }

// Generate schema — the generator will use System.Text.Json polymorphism metadata,
// which is now auto-populated by the resolver based on the interface's assembly:
var generator = new DefaultSchemaGenerator();
var schema = generator.Generate(typeof(IShape), jsonOptions);
```

Notes:
- Only the interface’s own assembly is scanned.
- No additional filtering is applied.
- The discriminator property name defaults to `type`, but you can override it via `UseAutoInterfacePolymorphism("type")`. 
