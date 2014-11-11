using Dynamo.DSEngine;
using Dynamo.Models;

using Autodesk.DesignScript.Runtime;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     DesignScript function node. All functions from DesignScript share the
    ///     same function node but internally have different procedure.
    /// </summary>
    [NodeName("Function Node"), NodeDescription("DesignScript Builtin Functions"),
     IsInteractive(false), IsVisibleInDynamoLibrary(false), NodeSearchable(false), IsMetaNode]
    public class DSFunction : DSFunctionBase
    {
        public DSFunction(FunctionDescriptor descriptor) 
            : base(new ZeroTouchNodeController<FunctionDescriptor>(descriptor)) 
        { }
    }
}
