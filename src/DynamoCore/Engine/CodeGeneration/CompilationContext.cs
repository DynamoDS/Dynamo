namespace Dynamo.Engine.CodeGeneration
{
    /// <summary>
    /// The context of AST compilation
    /// </summary>
    public enum CompilationContext
    {
        /// <summary>
        /// No specific context.
        /// </summary>
        None,

        /// <summary>
        /// Compiled AST nodes finally will be executed.
        /// </summary>
        DeltaExecution,

        /// <summary>
        /// Compiled AST nodes used in node to code.
        /// </summary>
        NodeToCode,

        /// <summary>
        /// Compiled AST nodes used in previwing graph.
        /// </summary>
        PreviewGraph,
    }
}
