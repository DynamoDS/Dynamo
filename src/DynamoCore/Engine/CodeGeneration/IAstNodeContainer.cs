using System;
using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Engine.CodeGeneration
{
    /// <summary>
    /// Returns notification for AST compilation events.                                                 
    /// </summary>
    public interface IAstNodeContainer
    {
        /// <summary>
        /// Indicates to start compiling a NodeModel to AST nodes.
        /// </summary>
        /// <param name="nodeGuid"></param>
        void OnCompiling(Guid nodeGuid);

        /// <summary>
        /// Indicates a NodeModel has been compiled to AST nodes. 
        /// </summary>
        /// <param name="nodeGuid"></param>
        /// <param name="astNodes"></param>
        void OnCompiled(Guid nodeGuid, IEnumerable<AssociativeNode> astNodes);
    }
}
