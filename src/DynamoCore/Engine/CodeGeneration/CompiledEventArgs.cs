using System;
using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Engine.CodeGeneration
{
    /// <summary>
    /// This event is triggered when a NodeModel has been compiled to a list of
    /// AST nodes.
    /// </summary>
    public class CompiledEventArgs : EventArgs
    {
        private Guid nodeId;
        
        /// <summary>
        /// Construct ASTBuiltEventArgs with NodeModel and AST nodes.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="astNodes"></param>
        internal CompiledEventArgs(Guid node, IEnumerable<AssociativeNode> astNodes)
        {
            nodeId = node;
            AstNodes = astNodes;
        }

        /// <summary>
        /// Guid of node that has been built to AST nodes.
        /// </summary>
        [Obsolete("This item is being obsoleted due to the confusing namimg, the new property to use is NodeId")]
        public Guid Node
        {
            get
            {
                return nodeId;
            }
        }

        /// <summary>
        /// Guid of node that has been built to AST nodes.
        /// </summary>
        public Guid NodeId
        {
            get
            {
                return nodeId;
            }
        }

        /// <summary>
        /// Built AST nodes.
        /// </summary>
        public IEnumerable<AssociativeNode> AstNodes { get; private set; }
    }
}
