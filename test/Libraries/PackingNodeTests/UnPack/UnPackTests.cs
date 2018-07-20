using Dynamo.Graph.Nodes;
using NUnit.Framework;
using PackingNodeModels.UnPack;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMDataBridge;

namespace Dynamo.Tests
{
    public class UnPackTests : DynamoModelTestBase
    {
        private string testFileWithPackUnPackNodes = Path.Combine(TestDirectory, @"core\PackingNode", "packingnodes.dyn");
        private const string typeDefinitionString = "Type A { prop:Type1 }";

        private UnPack GetUnPackNode()
        {
            return CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<UnPack>().First();
        }

        private void HookUpTypeDefinition(UnPack node, string typeString = typeDefinitionString)
        {
            node.InputNodes[0] = new Tuple<int, Graph.Nodes.NodeModel>(0, null);
            DataBridge.BridgeData(node.GUID.ToString(), new ArrayList { typeString });
        }

        public class Constructor : UnPackTests
        {
            [Test]
            public void ArgumentLacing_SetToLongest()
            {
                var node = new UnPack();
                Assert.That(node.ArgumentLacing, Is.EqualTo(LacingStrategy.Longest));
            }
        }

        public class DataBridgeCallBack : UnPackTests
        {
            [Test]
            public void TypeDefinitionChange_ShouldAddPorts()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var unpackNode = GetUnPackNode();

                unpackNode.RequestScheduledTask += (action) => action();

                Assert.That(unpackNode.OutPorts.Count, Is.EqualTo(0));

                HookUpTypeDefinition(unpackNode, "Type Something { prop1: Type1, prop2: Type2[] }");

                Assert.That(unpackNode.OutPorts.Count, Is.EqualTo(2));
                Assert.That(unpackNode.OutPorts[0].Name, Is.EqualTo("prop1"));
                Assert.That(unpackNode.OutPorts[0].ToolTip, Is.EqualTo("Type1"));
                Assert.That(unpackNode.OutPorts[1].Name, Is.EqualTo("prop2"));
                Assert.That(unpackNode.OutPorts[1].ToolTip, Is.EqualTo("Type2[]"));
            }

            [Test]
            public void TypeDefinitionChangeToNull_ShouldRemovePorts()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var unpackNode = GetUnPackNode();

                unpackNode.RequestScheduledTask += (action) => action();

                HookUpTypeDefinition(unpackNode, "Type Something { prop1: Type1, prop2: Type2[] }");

                Assert.That(unpackNode.OutPorts.Count, Is.EqualTo(2));

                unpackNode.InputNodes.Clear();
                DataBridge.BridgeData(unpackNode.GUID.ToString(), new ArrayList { "" });

                Assert.That(unpackNode.OutPorts.Count, Is.EqualTo(0));
            }
        }

        public class BuildOutPutAstMethod : UnPackTests
        {
            [Test]
            public void NullNodeInsideNodeList_ReturnsNullNode()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var unpackNode = GetUnPackNode();
                unpackNode.InPorts[0].Connectors.Add(new Graph.Connectors.ConnectorModel(new PortModel(PortType.Input, unpackNode, new PortData(null, null)), new PortModel(PortType.Input, unpackNode, new PortData(null, null)), Guid.NewGuid()));

                HookUpTypeDefinition(unpackNode);

                unpackNode.State = ElementState.Active;

                var output = unpackNode.BuildOutputAst(new List<AssociativeNode> { AstFactory.BuildStringNode("DST"), AstFactory.BuildNullNode() });

                Assert.That(output.ElementAt(1), Is.InstanceOf<BinaryExpressionNode>());
                var binaryOutput = output.ElementAt(1) as BinaryExpressionNode;
                Assert.That(binaryOutput.RightNode, Is.InstanceOf<NullNode>());
            }

            [Test]
            public void ValidData_ShouldOutPutOneFunctionCallPerTypeProperty()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var unpackNode = GetUnPackNode();
                unpackNode.RequestScheduledTask += (action) => action();
                unpackNode.InPorts[0].Connectors.Add(new Graph.Connectors.ConnectorModel(new PortModel(PortType.Input, unpackNode, new PortData(null, null)), new PortModel(PortType.Input, unpackNode, new PortData(null, null)), Guid.NewGuid()));

                HookUpTypeDefinition(unpackNode, "Type Something { prop1: Type1, prop2: Type2[]}");

                var output = unpackNode.BuildOutputAst(new List<AssociativeNode> { AstFactory.BuildStringNode("DST"), AstFactory.BuildStringNode("In") });

                Assert.That(output.Count(), Is.EqualTo(3)); //1 DataBridge and 2 properties;
                Assert.That(((output.ElementAt(1) as BinaryExpressionNode).RightNode as IdentifierListNode).RightNode, Is.InstanceOf<FunctionCallNode>());
                Assert.That(((output.ElementAt(2) as BinaryExpressionNode).RightNode as IdentifierListNode).RightNode, Is.InstanceOf<FunctionCallNode>());
            }
        }
    }
}
