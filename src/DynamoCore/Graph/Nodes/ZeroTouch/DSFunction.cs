using Autodesk.DesignScript.Runtime;
using Dynamo.Engine;

namespace Dynamo.Graph.Nodes.ZeroTouch
{
    /// <summary>
    ///     DesignScript function node. All functions from DesignScript share the
    ///     same function node but internally have different procedure.
    /// </summary>
    [NodeName("Function Node"), NodeDescription("DSFunctionNodeDescription", typeof(Properties.Resources)),
    IsVisibleInDynamoLibrary(false), IsMetaNode]
    [AlsoKnownAs("Dynamo.Nodes.DSFunction")]
    public class DSFunction : DSFunctionBase
    {
        /// <summary>
        ///     Indicates whether Node is input or not.
        /// </summary>
        public override bool IsInputNode
        {
            get { return false; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DSFunction"/> class.
        /// </summary>
        /// <param name="descriptor">Function descritor.</param>
        public DSFunction(FunctionDescriptor descriptor) 
            : base(new ZeroTouchNodeController<FunctionDescriptor>(descriptor)) 
        { }
    }
}

