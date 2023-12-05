using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodeModels.Logic
{
    [NodeName("Gate")]
    [NodeDescription(nameof(Resources.GateDescription), typeof(Resources))]
    [NodeCategory(BuiltinNodeCategories.LOGIC)]
    [NodeSearchTags(nameof(Resources.GateSearchTags), typeof(Resources))]
    [InPortNames(">")]
    [InPortTypes("object")]
    [InPortDescriptions(typeof(Resources), nameof(Resources.GateInPortToolTip))]
    [OutPortNames(">")]
    [OutPortTypes("object")]
    [OutPortDescriptions(typeof(Resources), nameof(Resources.GateOutPortToolTip))]
    [IsDesignScriptCompatible]
    public class Gate : NodeModel
    {
        private bool value;

        [JsonProperty("InputValue")]
        public virtual bool Value
        {
            get
            {
                return value;
            }
            set
            {
                if (!this.value.Equals(value))
                {
                    this.value = value;
                    ClearDirtyFlag();
                    OnNodeModified();
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }

        [JsonConstructor]
        private Gate(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = false;
        }

        public Gate()
        {
            Value = false;
            RegisterAllPorts();
            ShouldDisplayPreviewCore = false;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // Check that node can run
            if (!Value)
            {
                return new[]
                    {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode())};
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputAstNodes[0]) };
        }
    }
}
