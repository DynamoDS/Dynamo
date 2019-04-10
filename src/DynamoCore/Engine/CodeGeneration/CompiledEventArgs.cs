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
        /// <summary>
        /// Construct ASTBuiltEventArgs with NodeModel and AST nodes.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="astNodes"></param>
        internal CompiledEventArgs(Guid node, IEnumerable<AssociativeNode> astNodes)
        {
            Node = node;
            AstNodes = astNodes;
        }

        /// <summary>
        /// Guid of node that has been built to AST nodes.
        /// </summary>
        public Guid Node { get; private set; }

        /// <summary>
        /// Built AST nodes.
        /// </summary>
        public IEnumerable<AssociativeNode> AstNodes { get; private set; }
    }
}
