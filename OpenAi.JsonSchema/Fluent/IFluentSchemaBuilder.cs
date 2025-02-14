using System.Linq.Expressions;
using OpenAi.JsonSchema.Nodes;


namespace OpenAi.JsonSchema.Fluent;

public interface IFluentSchemaBuilder {
    SchemaNode Value<T>();
    SchemaNode Value(Type type);

    SchemaNode Object(string description, Action<IFluentObjectSchemaBuilder> properties);
    SchemaNode Object(Action<IFluentObjectSchemaBuilder> properties);

    SchemaNode Object<T>() where T : class;
    SchemaNode Object<T>(string description) where T : class;
    SchemaNode Object<T>(string description, Action<IFluentObjectSchemaBuilder<T>> properties) where T : class;
    SchemaNode Object<T>(Action<IFluentObjectSchemaBuilder<T>> properties) where T : class;

    SchemaNode Array<T>();
    SchemaNode Array<T>(string description);
    SchemaNode Array<T>(string description, Action<IFluentObjectSchemaBuilder<T>> properties) where T : class;
    SchemaNode Array<T>(Action<IFluentObjectSchemaBuilder<T>> properties) where T : class;

    SchemaNode AnyOf(params Func<IFluentSchemaBuilder, SchemaNode>[] values);
    SchemaNode AnyOf(string description, params Func<IFluentSchemaBuilder, SchemaNode>[] values);
    SchemaNode Const<T>(T value);
}

public interface IFluentObjectSchemaBuilder<T> {
    IFluentObjectSchemaBuilder<T> Property<TValue>(Expression<Func<T, TValue>> property);
    IFluentObjectSchemaBuilder<T> Property<TValue>(Expression<Func<T, TValue>> property, string description);
    IFluentObjectSchemaBuilder<T> Property<TValue>(Expression<Func<T, TValue>> property, string description, Func<IFluentSchemaBuilder, SchemaNode> value);
    IFluentObjectSchemaBuilder<T> Property<TValue>(Expression<Func<T, TValue>> property, Func<IFluentSchemaBuilder, SchemaNode> value);
    IFluentObjectSchemaBuilder<T> Property<TValue>(string property, string description, TValue value);
    IFluentObjectSchemaBuilder<T> Description(string description);
    IFluentObjectSchemaBuilder<T> Nullable(bool nullable);
}

public interface IFluentObjectSchemaBuilder {
    IFluentObjectSchemaBuilder Property<TValue>(string property);
    IFluentObjectSchemaBuilder Property<TValue>(string property, string description);
    IFluentObjectSchemaBuilder Property(string property, string description, Func<IFluentSchemaBuilder, SchemaNode> value);
    IFluentObjectSchemaBuilder Property(string property, Func<IFluentSchemaBuilder, SchemaNode> value);
    IFluentObjectSchemaBuilder Property<TValue>(string property, string description, TValue value);
    IFluentObjectSchemaBuilder Description(string description);
    IFluentObjectSchemaBuilder Nullable(bool nullable);
}
