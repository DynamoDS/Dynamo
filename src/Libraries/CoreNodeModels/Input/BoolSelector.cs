using System.Collections.Generic;
using CoreNodeModels.Properties;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels.Input
{
    public abstract class Bool : BasicInteractive<bool>
    {
        [JsonConstructor]
        protected Bool(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        protected Bool() : base()
        {
        }

        protected override bool DeserializeValue(string val)
        {
            try
            {
                return val.ToLower().Equals("true");
            }
            catch
            {
                return false;
            }
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            if (updateValueParams.PropertyName == "Value")
            {
                Value = DeserializeValue(updateValueParams.PropertyValue);
                return true;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        public override NodeInputData InputData
        {

            get
            {
                return new NodeInputData()
                {
                    Id = this.GUID,
                    Name = this.Name,
                    Type = NodeInputData.getNodeInputTypeFromType(typeof(System.Boolean)),
                    Description = this.Description,
                    Value = Value.ToString().ToLower(),
                };
            }
        }

        protected override string SerializeValue()
        {
            return Value.ToString();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildBooleanNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    [NodeName("Boolean")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("BooleanDescription", typeof(Resources))]
    [NodeSearchTags("BooleanSelectorSearchTags", typeof(Resources))]
    [OutPortTypes("bool")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.BoolSelector", "DSCoreNodesUI.Input.BoolSelector", "Dynamo.Nodes.BoolSelector")]
    public class BoolSelector : Bool
    {
        [JsonConstructor]
        private BoolSelector(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = false;
        }

        public BoolSelector()
        {
            Value = false;
            ShouldDisplayPreviewCore = false;
        }

        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "BooleanInputNode";
            }
        }
    }
}
