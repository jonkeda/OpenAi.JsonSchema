using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace OpenAi.JsonSchema.Serialization;

public sealed class AutoPolymorphismJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    private readonly IJsonTypeInfoResolver _inner;
    private readonly string _discriminatorPropertyName;

    public AutoPolymorphismJsonTypeInfoResolver(
        IJsonTypeInfoResolver? inner = null,
        string discriminatorPropertyName = "type")
    {
        _inner = inner ?? new DefaultJsonTypeInfoResolver();
        _discriminatorPropertyName = discriminatorPropertyName;
    }

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var info = _inner.GetTypeInfo(type, options);
        if (info is null)
            return null;

        // Only auto-handle interfaces, as requested.
        if (!type.IsInterface)
            return info;

        // Create or use existing polymorphism options.
        info.PolymorphismOptions ??= new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = _discriminatorPropertyName
        };

        // Scan only the interface's assembly for implementations.
        var assembly = type.Assembly;

        // Add all non-abstract classes assignable to the interface.
        foreach (var impl in GetImplementationsInAssembly(type, assembly))
        {
            // Avoid duplicates if already present (via attributes or earlier config).
            if (!IsAlreadyRegistered(info.PolymorphismOptions, impl))
            {
                var discriminator = GetDiscriminatorValue(impl);
                info.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(impl, discriminator));
            }
        }

        return info;
    }

    private static IEnumerable<Type> GetImplementationsInAssembly(Type iface, Assembly assembly)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
        }

        foreach (var t in types)
        {
            if (t.IsClass && !t.IsAbstract && iface.IsAssignableFrom(t))
                yield return t;
        }
    }

    private static bool IsAlreadyRegistered(JsonPolymorphismOptions options, Type impl)
    {
        foreach (var dt in options.DerivedTypes)
        {
            if (dt.DerivedType == impl)
                return true;
        }
        return false;
    }

    private static string GetDiscriminatorValue(Type t)
    {
        // Use simple type name; trim generic arity suffix if present.
        var name = t.Name;
        var tick = name.IndexOf('`');
        return tick >= 0 ? name.Substring(0, tick) : name;
    }
}