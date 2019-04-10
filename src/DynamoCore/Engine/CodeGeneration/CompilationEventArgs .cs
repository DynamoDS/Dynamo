using System;
using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Engine.CodeGeneration
{
    /// <summary>
    /// This class contains data about NodeModel compilation when a NodeModel is being 
    /// compiled to a list of AST nodes.
    /// </summary>
    public class CompilationEventArgs : EventArgs
    {
        private List<AssociativeNode> additionalAsts = new List<AssociativeNode>();

        /// <summary>
        /// Creates CompilationEventArgs
        /// </summary>
        /// <param name="node">The node being compiled.</param>
        /// <param name="inputNodes">List of input ast nodes.</param>
        /// <param name="context">Compilation context.</param>
        public CompilationEventArgs(Guid node, IEnumerable<AssociativeNode> inputNodes, CompilationContext context)
        {
            Node = node;
            InputAstNodes = inputNodes;
            Context = context;
        }

        /// <summary>
        /// Returns Guid of the node being compiled.
        /// </summary>
        public Guid Node { get; private set; }

        /// <summary>
        /// Returns a list of additional ast nodes as added by the event handlers.
        /// </summary>
        public IEnumerable<AssociativeNode> AdditionalAstNodes { get { return additionalAsts; } }

        /// <summary>
        /// Returns input ast nodes as passed to the node being compiled.
        /// </summary>
        public IEnumerable<AssociativeNode> InputAstNodes { get; private set; }

        /// <summary>
        /// Provides an oppotunity to the event handlers to add additional AST nodes
        /// during the compilation.
        /// </summary>
        /// <param name="node">Additonal AST node to add.</param>
        public void AddAstNode(AssociativeNode node)
        {
            additionalAsts.Add(node);
        }

        /// <summary>
        /// Compilation context
        /// </summary>
        public CompilationContext Context { get; private set; }
    }
}
