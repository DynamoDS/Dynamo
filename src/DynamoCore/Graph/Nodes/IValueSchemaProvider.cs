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
        /// Type identifier matching the Player ValueSchema.TypeId wire format.
        /// For geometry/complex types: Forge Data Schema URN (e.g. "autodesk.math:point3d-1.0.0").
        /// For primitives: ForgeDataSchemaType values (e.g. "Float64", "String", "Bool", "Int64").
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
