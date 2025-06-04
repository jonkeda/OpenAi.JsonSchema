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

        // System.InvalidOperationException : Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.
        bool seen;
        lock (_seen) {
            seen = _seen.Add(schema.Ref.Value);
        }

        if (seen) {
            Visit(schema.Ref.Value);
        }
    }
}
