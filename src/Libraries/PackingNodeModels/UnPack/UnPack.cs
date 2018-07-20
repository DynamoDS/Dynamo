using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using PackingNodeModels.Properties;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackingNodeModels.UnPack
{
    /// <summary>
    /// Creates a node that takes in a Dictionary (or ArrayList of Dictionary) and a TypeDefinition to output the unpacked data from the dictionary.
    /// </summary>
    [NodeName("UnPack")]
    [NodeCategory(BuiltinNodeCategories.CORE_PACKING)]
    [NodeDescription("UnPackNodeDescription", typeof(Resource))]
    [IsDesignScriptCompatible]
    public class UnPack : PackingNode
    {
        public UnPack()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData(Resource.UnPackNodeDataInputName, Resource.UnPackNodeDataInputType)));

            ArgumentLacing = LacingStrategy.Longest;
        }

        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        [JsonConstructor]
        protected UnPack(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
        
        protected override void RefreshTypeDefinitionPorts()
        {
            OutPorts.ToList().ForEach(portModel => OutPorts.Remove(portModel));

            if (TypeDefinition != null)
            {
                foreach (var property in TypeDefinition.Properties)
                {
                    OutPorts.Add(new PortModel(PortType.Output, this, new PortData(property.Key, property.Value.ToString())));
                }
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var baseOutput = base.BuildOutputAst(inputAstNodes).ToList();

            if (!IsValidInputState(inputAstNodes))
            {
                baseOutput.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()));
                return baseOutput;
            }

            UseLevelAndReplicationGuide(inputAstNodes);

            foreach (var property in TypeDefinition.Properties)
            {
                int index = TypeDefinition.Properties.ToList().IndexOf(property);
                var functionCall = AstFactory.BuildFunctionCall(
                    new Func<DesignScript.Builtin.Dictionary, string, object>(DSCore.UnPackFunctions.UnPackOutputByKey),
                    new List<AssociativeNode> { inputAstNodes[1], AstFactory.BuildStringNode(property.Key) });
                baseOutput.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(index), functionCall));
            }

            return baseOutput;
        }
    }
}
