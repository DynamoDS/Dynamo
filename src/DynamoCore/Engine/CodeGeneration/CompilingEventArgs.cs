using System;

namespace Dynamo.Engine.CodeGeneration
{
    /// <summary>
    /// This event is triggerred when compiling a NodeModel to AST nodes.
    /// </summary>
    public class CompilingEventArgs : EventArgs
    {
        /// Construct ASTBuildingEventArgs with NodeModel.
        public CompilingEventArgs(Guid node)
        {
            Node = node;
        }

        /// <summary>
        /// Guid of NodeModel that is being compiled to AST.
        /// </summary>
        public Guid Node { get; private set; }
    }
}
