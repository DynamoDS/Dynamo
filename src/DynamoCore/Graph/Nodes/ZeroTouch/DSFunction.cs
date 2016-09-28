using Autodesk.DesignScript.Runtime;
using Dynamo.Engine;
using Newtonsoft.Json;
using System.Collections.Generic;

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
        public FunctionDescriptor FunctionDescription{get;set;}

        /// <summary>
        ///     Indicates whether Node is input or not.
        /// </summary>
        public override bool IsInputNode
        {
            get { return false; }
        }

        [JsonConstructor]
        private DSFunction(FunctionDescriptor functionDescription, 
            IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
            base(new ZeroTouchNodeController<FunctionDescriptor>(functionDescription),
                inPorts, outPorts)
        {
            FunctionDescription = functionDescription;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DSFunction"/> class.
        /// </summary>
        /// <param name="description">Function descritor.</param>
        public DSFunction(FunctionDescriptor functionDescription) 
            : base(new ZeroTouchNodeController<FunctionDescriptor>(functionDescription)) 
        {
            FunctionDescription = functionDescription;
        }
    }
}

