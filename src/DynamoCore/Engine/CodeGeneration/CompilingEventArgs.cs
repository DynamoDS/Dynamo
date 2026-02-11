using System;

namespace Dynamo.Engine.CodeGeneration
{
    /// <summary>
    /// This event is triggerred when compiling a NodeModel to AST nodes.
    /// </summary>
    public class CompilingEventArgs : EventArgs
    {
        private Guid nodeId;

        /// Construct ASTBuildingEventArgs with NodeModel.
        public CompilingEventArgs(Guid node)
        {
            nodeId = node;
        }

        /// <summary>
        /// Guid of NodeModel that is being compiled to AST.
        /// </summary>
        public Guid NodeId
        {
            get
            {
                return nodeId;
            }
        }
    }
}
