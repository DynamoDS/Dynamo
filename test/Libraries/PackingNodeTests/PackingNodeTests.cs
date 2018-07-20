using Dynamo.Graph.Nodes;
using NUnit.Framework;
using PackingNodeModels;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VMDataBridge;

namespace Dynamo.Tests
{
    public class PackingNodeTests : DynamoModelTestBase
    {
        private string testFileWithPackUnPackNodes = Path.Combine(TestDirectory, @"core\PackingNode", "packingnodes.dyn");
        private const string typeDefinitionString = "Type A { prop:Type1 }";

        private PackingNode GetPackingNode()
        {
            return CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<PackingNode>().First();
        }

        private void HookUpTypeDefinition(PackingNode node, string typeString = typeDefinitionString)
        {
            node.InputNodes[0] = new Tuple<int, Graph.Nodes.NodeModel>(0, null);
            DataBridge.BridgeData(node.GUID.ToString(), new ArrayList { typeString });
        }

        public class Constructor : PackingNodeTests
        {
            [Test]
            public void DefaultCtor_ShouldAddTypeDefinitionInport()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                Assert.That(packingNode.InPorts.Count, Is.GreaterThan(0));
                Assert.That(packingNode.InPorts[0].Name, Is.EqualTo("Type"));
                Assert.That(packingNode.InPorts[0].ToolTip, Is.EqualTo("TypeDefinition as a string"));
                Assert.That(packingNode.InPorts[0].PortType, Is.EqualTo(PortType.Input));
            }
        }

        public class TypeDefinitionProperty : PackingNodeTests
        {
            [Test]
            public void SettingValue_ShouldClearErrorsAndWarnings()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                packingNode.Error("some error");

                Assert.True(packingNode.IsInErrorState);

                HookUpTypeDefinition(packingNode);

                Assert.False(packingNode.IsInErrorState);
            }

            [Test]
            public void SettingValue_ShouldRequestScheduledTask_WithRefreshTypeDefinitionPortsMethod()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();
                var requested = false;
                var methodName = "";
                packingNode.RequestScheduledTask += (action) => { methodName = action.Method.Name; requested = true; };

                HookUpTypeDefinition(packingNode);

                Assert.IsTrue(requested);
                Assert.AreEqual("RefreshTypeDefinitionPorts", methodName);
            }
        }

        public class IsInValidStateProperty : PackingNodeTests
        {
            [Test]
            public void Getter_ShouldConjunctErrorStateAndWarningStates()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                packingNode.State = ElementState.Error;
                Assert.False(packingNode.IsInValidState);

                packingNode.State = ElementState.Warning;
                Assert.False(packingNode.IsInValidState);

                packingNode.State = ElementState.PersistentWarning;
                Assert.False(packingNode.IsInValidState);

                packingNode.State = ElementState.Active;
                Assert.True(packingNode.IsInValidState);
            }
        }

        public class BuildOutputAstMethod : PackingNodeTests
        {
            [Test]
            public void ShouldReturnDataBridgeAst()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                var astOutput = packingNode.BuildOutputAst(null);

                Assert.That(astOutput.Count(), Is.GreaterThanOrEqualTo(1));
                Assert.That(astOutput.ElementAt(0), Is.InstanceOf<BinaryExpressionNode>());

                var binaryExpressionNode = astOutput.ElementAt(0) as BinaryExpressionNode;
                Assert.That(binaryExpressionNode.RightNode, Is.InstanceOf<IdentifierListNode>());

                var identifierListNode = binaryExpressionNode.RightNode as IdentifierListNode;
                Assert.That(identifierListNode.RightNode, Is.InstanceOf<FunctionCallNode>());

                var functionCallNode = identifierListNode.RightNode as FunctionCallNode;
                Assert.That("BridgeData", Is.EqualTo(functionCallNode.Function.Name));
            }
        }

        public class DataBridgeCallBackMethod : PackingNodeTests
        {
            [Test]
            public void InputNodesCountIsZero_ShouldSetTypeDefinitionToNull()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                Assert.That(packingNode.InputNodes.Count, Is.EqualTo(0));

                DataBridge.BridgeData(packingNode.GUID.ToString(), new ArrayList { "" });

                Assert.IsNull(packingNode.TypeDefinition);
            }

            [Test]
            public void InputNodesIndex0NotDefined_ShouldSetTypeDefinitionToNull()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                Assert.That(packingNode.InputNodes.Count, Is.EqualTo(0));

                packingNode.InputNodes[1] = new Tuple<int, Graph.Nodes.NodeModel>(1, null);

                DataBridge.BridgeData(packingNode.GUID.ToString(), new ArrayList { "" });

                Assert.IsNull(packingNode.TypeDefinition);
            }

            [Test]
            public void InputNodesIndex0IsNull_ShouldSetTypeDefinitionToNull()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                Assert.That(packingNode.InputNodes.Count, Is.EqualTo(0));

                packingNode.InputNodes[0] = null;

                DataBridge.BridgeData(packingNode.GUID.ToString(), new ArrayList { "" });

                Assert.IsNull(packingNode.TypeDefinition);
            }

            [Test]
            public void TypeValueIsNotString_ShouldAddAWarning()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                Assert.That(packingNode.State, Is.Not.EqualTo(ElementState.PersistentWarning));

                packingNode.InputNodes[0] = new Tuple<int, Graph.Nodes.NodeModel>(0, null);

                DataBridge.BridgeData(packingNode.GUID.ToString(), new ArrayList { 1 });
                
                Assert.False(packingNode.IsInValidState);
                Assert.That(packingNode.State, Is.EqualTo(ElementState.PersistentWarning));
            }

            [Test]
            public void TypeValueIsInvalidTypeString_ShouldAddAWarning()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                Assert.That(packingNode.State, Is.Not.EqualTo(ElementState.PersistentWarning));

                packingNode.InputNodes[0] = new Tuple<int, Graph.Nodes.NodeModel>(0, null);

                DataBridge.BridgeData(packingNode.GUID.ToString(), new ArrayList { "some invalid type definition string" });

                Assert.False(packingNode.IsInValidState);
                Assert.That(packingNode.State, Is.EqualTo(ElementState.PersistentWarning));
            }

            [Test]
            public void TypeValueIsValidTypeString_ShouldUpdateTypeDefinition()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packingNode = GetPackingNode();

                Assert.Null(packingNode.TypeDefinition);

                HookUpTypeDefinition(packingNode, "Type Something { prop1: Type1, prop2: Type2[] }");

                Assert.That(packingNode.TypeDefinition.Name, Is.EqualTo("Something"));
                Assert.That(packingNode.TypeDefinition.Properties.Count, Is.EqualTo(2));
                Assert.That(packingNode.TypeDefinition.Properties.ElementAt(0).Key, Is.EqualTo("prop1"));
                Assert.That(packingNode.TypeDefinition.Properties.ElementAt(0).Value.Type, Is.EqualTo("Type1"));
                Assert.That(packingNode.TypeDefinition.Properties.ElementAt(0).Value.IsCollection, Is.EqualTo(false));
                Assert.That(packingNode.TypeDefinition.Properties.ElementAt(1).Key, Is.EqualTo("prop2"));
                Assert.That(packingNode.TypeDefinition.Properties.ElementAt(1).Value.Type, Is.EqualTo("Type2"));
                Assert.That(packingNode.TypeDefinition.Properties.ElementAt(1).Value.IsCollection, Is.EqualTo(true));
            }
        }
    }
}
