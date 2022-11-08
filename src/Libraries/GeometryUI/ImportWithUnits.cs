using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System.Collections.Generic;
using System.Linq;

namespace GeometryUI
{
    [NodeCategory(BuiltinNodeCategories.GEOMETRY)]
    [NodeName("Geometry.ImportFromSATWithUnits")]
    [InPortTypes(new string[] { "var", "DynamoUnits.Unit" })]
    [InPortNames(new[] { "file|filePath","dynamoUnit" })]
    [InPortDescriptions(typeof(Properties.Resources),new[] { "ImportSATFilePathDesc", "ImportSATDynamoUnitDesc"})]
    [NodeDescription("ImportToSATUnitsDesc", typeof(Properties.Resources))]
    [OutPortTypes("Geometry[]")]
    [OutPortNames("geometry")]
    [OutPortDescriptions(typeof(Properties.Resources), "SABSATGeoDesc")]
    [IsDesignScriptCompatible]
    public class ImportFromSATWithUnits : NodeModel
    {
        [JsonConstructor]
        private ImportFromSATWithUnits(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
    base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = true;
            inPorts.ElementAt(1).DefaultValue = new NullNode();
        }

        public ImportFromSATWithUnits()
        {
            ShouldDisplayPreviewCore = true;
            ArgumentLacing = LacingStrategy.Auto;
            RegisterAllPorts();
            //setting port data via attributes don't currently support default vals
            InPorts.ElementAt(1).DefaultValue = new NullNode();
            InPorts.ElementAt(1).UsingDefaultValue = true;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
          List<AssociativeNode> inputAstNodes)
        {
            var funcNode = AstFactory.BuildFunctionCall("ImportHelpers", "ImportFromSATWithUnits",
                        new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1] }) as IdentifierListNode;

            if (IsPartiallyApplied)
            {
                return new[]
                {
                    
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildFunctionObject(
                            new IdentifierListNode(){LeftNode = funcNode?.LeftNode,
                            RightNode = (funcNode.RightNode as FunctionCallNode)?.Function },
                            InPorts.Count,
                            Enumerable.Range(0, InPorts.Count).Where(index=>InPorts[index].IsConnected),
                            inputAstNodes))
                };
            }
            else
            {
                UseLevelAndReplicationGuide(inputAstNodes);

                return new[]
                {
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),funcNode)
                };
            }
        }
    }

    [NodeCategory(BuiltinNodeCategories.GEOMETRY)]
    [NodeName("Geometry.DeserializeFromSABWithUnits")]
    [InPortTypes(new string[] { "byte[]", "DynamoUnits.Unit" })]
    [InPortNames(new[] { "buffer", "dynamoUnit" })]
    [InPortDescriptions(typeof(Properties.Resources), new[] { "ImportSABByteArrayDesc", "ImportSATDynamoUnitDesc" })]
    [NodeDescription("ImportToSABUnitsDesc", typeof(Properties.Resources))]
    [OutPortTypes("Geometry[]")]
    [OutPortNames("geometry")]
    [OutPortDescriptions(typeof(Properties.Resources), "SABSATGeoDesc")]
    [IsDesignScriptCompatible]
    public class DeserializeFromSABWithUnits : NodeModel
    {
        [JsonConstructor]
        private DeserializeFromSABWithUnits(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :
    base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = true;
            inPorts.ElementAt(1).DefaultValue = new NullNode();
        }

        public DeserializeFromSABWithUnits()
        {
            ShouldDisplayPreviewCore = true;
            ArgumentLacing = LacingStrategy.Auto;
            RegisterAllPorts();
            //setting port data via attributes don't currently support default vals
            InPorts.ElementAt(1).DefaultValue = new NullNode();
            InPorts.ElementAt(1).UsingDefaultValue = true;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
          List<AssociativeNode> inputAstNodes)
        {
        
            var funcNode = AstFactory.BuildFunctionCall("ImportHelpers", "DeserializeFromSABWithUnits",
                          new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1] }) as IdentifierListNode;

            if (IsPartiallyApplied)
            {
                return new[]
                {

                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildFunctionObject(
                            new IdentifierListNode(){LeftNode = funcNode?.LeftNode,
                            RightNode = (funcNode.RightNode as FunctionCallNode)?.Function },
                            InPorts.Count,
                            Enumerable.Range(0, InPorts.Count).Where(index=>InPorts[index].IsConnected),
                            inputAstNodes))
                };
            }
            else
            {
                UseLevelAndReplicationGuide(inputAstNodes);

                return new[]
                {
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),funcNode)
                };
            }
        }
    }
}
