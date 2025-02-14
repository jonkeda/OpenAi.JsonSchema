using OpenAi.JsonSchema.Nodes;
using OpenAi.JsonSchema.Nodes.Abstractions;


namespace OpenAi.JsonSchema.Internals;

internal class SchemaRefCountVisitor : SchemaVisitor {
    public static SchemaRefCountVisitor Instance { get; } = new();

    private readonly HashSet<SchemaNode> _seen = [];

    public override void Visit(SchemaRootNode schema)
    {
        Visit(schema.Root);
    }

    public override void Visit(SchemaRefNode schema)
    {
        schema.Ref.Count++;
        if (_seen.Add(schema.Ref.Value)) {
            Visit(schema.Ref.Value);
        }
    }
}
