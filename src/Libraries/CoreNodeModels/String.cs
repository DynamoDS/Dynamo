using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels
{
    /// <summary>
    /// Base class to represent a single input string node. It supports 
    /// partially applied function. 
    /// </summary>
    public class ToStringNodeBase : NodeModel
    {
        [JsonConstructor]
        protected ToStringNodeBase(string functionName, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            this.functionName = functionName;
        }

        public ToStringNodeBase(string functionName)
        {
            this.functionName = functionName;
        }

        private string functionName;

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode rhs = null;

            //to avoid needing to add a different builtin function for stringFromObject conversion
            //we add a default value of false to old node calls, which will only have 1 parameter, the string.
            //new calls will have 2 parameters. the string and the numeric format option.
            if (inputAstNodes.Count < 2)
            {
                //force to false for use numeric to keep old behavior of this node.
                inputAstNodes.Add(AstFactory.BuildBooleanNode(false));
            }

            if (IsPartiallyApplied)
            {
                var connectedInputs = new List<AssociativeNode>();
                var functionNode = new IdentifierNode(functionName);
                var paramNumNode = new IntNode(1);
                var positionNode = AstFactory.BuildExprList(connectedInputs);
                var arguments = AstFactory.BuildExprList(inputAstNodes);
                var inputParams = new List<AssociativeNode>
                {
                    functionNode,
                    paramNumNode,
                    positionNode,
                    arguments,
                    AstFactory.BuildBooleanNode(true)
                };

                rhs = AstFactory.BuildFunctionCall("__CreateFunctionObject", inputParams);
            }
            else
            {  
                rhs = AstFactory.BuildFunctionCall(functionName, inputAstNodes);
            }

            return new[]
            {
               AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs)
            };
        }
    }

    [NodeName("String from Object2")]
    [NodeDescription("StringfromObjectDescription", typeof(Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromObjectSearchTags", typeof(Resources))]
    [OutPortTypes("string")]
    [IsDesignScriptCompatible]
    public class StringFromObject : ToStringNodeBase
    {
        [JsonConstructor]
        private StringFromObject(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
            base("__ToStringFromObject", inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public StringFromObject() : base("__ToStringFromObject")
        {
            ArgumentLacing = LacingStrategy.Disabled;
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("object", Resources.FromObjectPortDataObjToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this,
                    new PortData("useNumericFormat",
                        "should the numeric precision format be used when converting doubles",
                        AstFactory.BuildBooleanNode(false))));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("string", Resources.FromObjectPortDataResultToolTip)));
            RegisterAllPorts();
        }
    }

    [NodeName("String from Object")]
    [NodeDescription("StringfromObjectDescription", typeof(Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromObjectSearchTags", typeof(Resources))]
    [OutPortTypes("string")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.StringNodes.FromObject", "DSCoreNodesUI.FromObject")]
    [Obsolete("TODO update to resx - please use string from object overload with numeric format option")]
    public class FromObject: ToStringNodeBase 
    {
        [JsonConstructor]
        private FromObject(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : 
            base("__ToStringFromObject",inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }
        public FromObject() : base("__ToStringFromObject")
        {
            ArgumentLacing = LacingStrategy.Disabled;
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("object", Resources.FromObjectPortDataObjToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("string", Resources.FromObjectPortDataResultToolTip)));
            RegisterAllPorts();
        }
    }

    [NodeName("String from Array2")]
    [NodeDescription("StringfromArrayDescription", typeof(Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromArraySearchTags", typeof(Resources))]
    [OutPortTypes("string")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.StringNodes.FromArray", "DSCoreNodesUI.FromArray")]
    public class StringFromArray : ToStringNodeBase
    {
        [JsonConstructor]
        private StringFromArray(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
            base("__ToStringFromArray", inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public StringFromArray() : base("__ToStringFromArray")
        {
            ArgumentLacing = LacingStrategy.Disabled;
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("array", Resources.FromArrayPortDataArrayToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("string", Resources.FromArrayPortDataResultToolTip)));
            RegisterAllPorts();
        }
    }

    [NodeName("String from Array")]
    [NodeDescription("StringfromArrayDescription", typeof(Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromArraySearchTags", typeof(Resources))]
    [OutPortTypes("string")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.StringNodes.FromArray", "DSCoreNodesUI.FromArray")]
    public class FromArray : ToStringNodeBase 
    {
        [JsonConstructor]
        private FromArray(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : 
            base("__ToStringFromArray", inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public FromArray() : base("__ToStringFromArray")
        {
            ArgumentLacing = LacingStrategy.Disabled;
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("array", Resources.FromArrayPortDataArrayToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this,
                new PortData("useNumericFormat",
                    "should the numeric precision format be used when converting doubles",
                    AstFactory.BuildBooleanNode(false))));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("string", Resources.FromArrayPortDataResultToolTip)));
            RegisterAllPorts();
        }
    }
}
