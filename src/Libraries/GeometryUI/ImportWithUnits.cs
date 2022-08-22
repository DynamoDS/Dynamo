using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System.Collections.Generic;

namespace GeometryUI
{
    //TODO finish the descriptions etc with resx.

    [NodeCategory(BuiltinNodeCategories.GEOMETRY)]
    [NodeName("Geometry.ImportFromSatWithUnits")]
    [InPortTypes(new string[] { "var", "DynamoUnits.Unit" })]
    //[NodeDescription("ExportToSATDescripiton", typeof(GeometryUI.Properties.Resources))]
    //[NodeSearchTags("ExportWithUnitsSearchTags", typeof(GeometryUI.Properties.Resources))]
    [OutPortTypes("Geometry[]")]
    [IsDesignScriptCompatible]
    public class ImportFromSATAndUnits : NodeModel
    {
        [JsonConstructor]
        private ImportFromSATAndUnits(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
    base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = true;
        }

        public ImportFromSATAndUnits()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("file|filePath", "TODO")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("dynamoUnit", "TODO", new NullNode())));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("geometry", "TODO")));

            ShouldDisplayPreviewCore = true;
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
          List<AssociativeNode> inputAstNodes)
        {
            if (!InPorts[0].IsConnected || (!InPorts[1].IsConnected && !InPorts[1].UsingDefaultValue))
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var funcNode = AstFactory.BuildFunctionCall("ImportHelpers", "ImportFromSATAndUnits",
                        new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1] });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode) };
        }
    }

    [NodeCategory(BuiltinNodeCategories.GEOMETRY)]
    [NodeName("Geometry.DeserializeFromSABWithUnits")]
    [InPortTypes(new string[] { "byte[]", "DynamoUnits.Unit" })]
    //[NodeDescription("ExportToSATDescripiton", typeof(GeometryUI.Properties.Resources))]
    //[NodeSearchTags("ExportWithUnitsSearchTags", typeof(GeometryUI.Properties.Resources))]
    [OutPortTypes("Geometry[]")]
    [IsDesignScriptCompatible]
    public class ImportSABByUnits : NodeModel
    {
        [JsonConstructor]
        private ImportSABByUnits(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
    base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = true;
        }

        public ImportSABByUnits()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("bytes", "TODO")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("dynamoUnit", "TODO", new NullNode())));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("geometry", "TODO")));

            ShouldDisplayPreviewCore = true;
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
          List<AssociativeNode> inputAstNodes)
        {
            if (!InPorts[0].IsConnected || (!InPorts[1].IsConnected && !InPorts[1].UsingDefaultValue))
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var funcNode = AstFactory.BuildFunctionCall("ImportHelpers", "DeserializeFromSABAndUnits",
                        new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1] });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode) };
        }
    }
}
