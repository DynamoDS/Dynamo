using System;
using System.Collections.Generic;
using System.Linq;
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

    [NodeName("Formatted String from Object")]
    [NodeDescription("StringfromObjectDescription", typeof(Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromObjectSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [System.Diagnostics.CodeAnalysis.Experimental("NEWNODE_FormattedStringFromObject")]
    [InPortNames("object","formatSpecifier")]
    [InPortTypes("var", "string")]
    [InPortDescriptions(typeof(Resources), "FromObjectPortDataObjToolTip", "FromObjectPortDataFormatToolTip")]
    [OutPortNames("string")]
    [OutPortTypes("string")]
    [OutPortDescriptions(typeof(Resources),"FromObjectPortDataResultToolTip")]
    public class FormattedStringFromObject : ToStringNodeBase
    {
        [JsonConstructor]
        private FormattedStringFromObject(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
            base("__ToStringFromObjectAndFormat", inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            if (inPorts?.Count() > 1)
            {   
                inPorts.ElementAt(1).DefaultValue = AstFactory.BuildStringNode("G");
            }
        }

        public FormattedStringFromObject() : base("__ToStringFromObjectAndFormat")
        {
            ArgumentLacing = LacingStrategy.Disabled;
            RegisterAllPorts();
            //TODO figure out how to add an inportDefaultValue attribute.
            //it's not straightforward because ideally we'd have accesss to the parser.
            if (InPorts?.Count() > 1)
            {
                InPorts.ElementAt(1).DefaultValue = AstFactory.BuildStringNode("G");
                InPorts.ElementAt(1).UsingDefaultValue = true;
            }
        }
    }

    [NodeName("String from Object")]
    [NodeDescription("StringfromObjectDescription", typeof(Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromObjectSearchTags", typeof(Resources))]
    [OutPortTypes("string")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.StringNodes.FromObject", "DSCoreNodesUI.FromObject")]
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

    [NodeName("Formatted String from Array")]
    [NodeDescription("StringfromArrayDescription", typeof(Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromArraySearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [System.Diagnostics.CodeAnalysis.Experimental("NEWNODE_FormattedStringFromArray")]
    [InPortNames("array", "formatSpecifier")]
    [InPortTypes("var", "string")]
    [InPortDescriptions(typeof(Resources), "FromArrayPortDataArrayToolTip", "FromObjectPortDataFormatToolTip")]
    [OutPortNames("string")]
    [OutPortTypes("string")]
    [OutPortDescriptions(typeof(Resources),"FromArrayPortDataResultToolTip")]
    public class FormattedStringFromArray : ToStringNodeBase
    {
        [JsonConstructor]
        private FormattedStringFromArray(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
            base("__ToStringFromArrayAndFormat", inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            if (inPorts?.Count() > 1)
            {
                inPorts.ElementAt(1).DefaultValue = AstFactory.BuildStringNode("G");
            }
        }

        public FormattedStringFromArray() : base("__ToStringFromArrayAndFormat")
        {
            ArgumentLacing = LacingStrategy.Disabled;
            RegisterAllPorts();
            //TODO figure out how to add an inportDefaultValue attribute.
            //it's not straightforward because ideally we'd have accesss to the parser.
            if (InPorts?.Count() > 1)
            {
                InPorts.ElementAt(1).DefaultValue = AstFactory.BuildStringNode("G");
                InPorts.ElementAt(1).UsingDefaultValue = true;
            }
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
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("string", Resources.FromArrayPortDataResultToolTip)));
            RegisterAllPorts();
        }
    }
}
