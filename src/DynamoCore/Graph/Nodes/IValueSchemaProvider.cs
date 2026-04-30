namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// Implemented by NodeModels that can describe their value type
    /// for external consumers (DynamoPlayer, MCP, etc.).
    /// Returns a canonical Forge Data Schema type identifier and list context.
    /// </summary>
    public interface IValueSchemaProvider
    {
        /// <summary>
        /// Canonical type identifier in Forge Data Schema format.
        /// Examples: "autodesk.math:point3d-1.0.0", "String", "Float64".
        /// Returns a non-null type identifier. If the type is not yet determined,
        /// implementations should return their existing non-null fallback value.
        /// </summary>
        string ValueTypeId { get; }

        /// <summary>
        /// Whether the value is a list (array) of the declared type.
        /// </summary>
        bool IsListValue { get; }
    }
}
