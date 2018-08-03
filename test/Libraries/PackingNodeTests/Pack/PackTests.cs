using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PackingNodeModels.Pack;
using VMDataBridge;
using System.Collections;
using NUnit.Framework;
using Dynamo.Graph.Nodes;
using PackingNodeModels.Pack.Validation;
using Moq;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Tests
{
    public class PackTests : DynamoModelTestBase
    {
        private string testFileWithPackUnPackNodes = Path.Combine(TestDirectory, @"core\PackingNode", "packingnodes.dyn");
        private const string typeDefinitionString = "Type A { prop:Type1 }";

        private Pack GetPackNode()
        {
            return CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Pack>().First();
        }

        private void HookUpTypeDefinition(Pack node, string typeString = typeDefinitionString)
        {
            node.InputNodes[0] = new Tuple<int, Graph.Nodes.NodeModel>(0, null);
            DataBridge.BridgeData(node.GUID.ToString(), new ArrayList { typeString });
        }

        private void SetValidationManager(Pack node, IValidationManager validationManager)
        {
            var prop = node.GetType().GetField("validationManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop.SetValue(node, validationManager);
        }

        public class Constructor : PackTests
        {
            [Test]
            public void DefaultConstructor_ShouldAddDefaultOutport()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();

                Assert.That(packNode.OutPorts.Count, Is.EqualTo(1));
                Assert.That(packNode.OutPorts[0].Name, Is.EqualTo("Out"));
                Assert.That(packNode.OutPorts[0].ToolTip, Is.EqualTo("Dictionary"));
                Assert.That(packNode.OutPorts[0].PortType, Is.EqualTo(PortType.Output));
            }
        }

        public class DataBridgeCallBack : PackTests
        {
            [Test]
            public void TypeDefinitionChange_ShouldAddPorts()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();

                packNode.RequestScheduledTask += (action) => action();

                Assert.That(packNode.InPorts.Count, Is.EqualTo(1));

                HookUpTypeDefinition(packNode, "Type Something { prop1: Type1, prop2: Type2[] }");

                Assert.That(packNode.InPorts.Count, Is.EqualTo(3));
                Assert.That(packNode.InPorts[1].Name, Is.EqualTo("prop1"));
                Assert.That(packNode.InPorts[1].ToolTip, Is.EqualTo("Type1"));
                Assert.That(packNode.InPorts[2].Name, Is.EqualTo("prop2"));
                Assert.That(packNode.InPorts[2].ToolTip, Is.EqualTo("Type2[]"));
            }

            [Test]
            public void TypeDefinitionChangeToNull_ShouldRemovePorts()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();

                packNode.RequestScheduledTask += (action) => action();

                HookUpTypeDefinition(packNode, "Type Something { prop1: Type1, prop2: Type2[] }");

                Assert.That(packNode.InPorts.Count, Is.EqualTo(3));

                packNode.InputNodes.Clear();
                DataBridge.BridgeData(packNode.GUID.ToString(), new ArrayList { "" });

                Assert.That(packNode.InPorts.Count, Is.EqualTo(1));
            }
        }

        public class ValidateInputsMethod : PackTests
        {
            [Test]
            public void NoCachedValues_ShouldCallHandleValidationForEveryValue()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();
                var validationManagerMock = new Mock<IValidationManager>();
                validationManagerMock.Setup(mock => mock.HandleValidation(It.IsAny<Dictionary<int, object>>()));
                validationManagerMock.Setup(mock => mock.Warnings).Returns(new List<Validation>().AsReadOnly());
                SetValidationManager(packNode, validationManagerMock.Object);

                HookUpTypeDefinition(packNode, typeDefinitionString);

                DataBridge.BridgeData(packNode.GUID.ToString(), new ArrayList { typeDefinitionString, "someValue"});

                validationManagerMock.Verify(mock => mock.HandleValidation(It.Is<Dictionary<int, object>>(d => d.Count == 1)), Times.Once());
            }

            [Test]
            public void WithCachedValues_ShouldCallHandleValidationForUnCachedValues()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();
                var validationManagerMock = new Mock<IValidationManager>();
                validationManagerMock.Setup(mock => mock.HandleValidation(It.IsAny<Dictionary<int, object>>()));
                validationManagerMock.Setup(mock => mock.Warnings).Returns(new List<Validation>().AsReadOnly());
                SetValidationManager(packNode, validationManagerMock.Object);
                var typeDefinitionString = "Type Something { prop1: Type1, prop2: Type2 }";

                HookUpTypeDefinition(packNode, typeDefinitionString);

                DataBridge.BridgeData(packNode.GUID.ToString(), new ArrayList { typeDefinitionString, "someValue" });
                validationManagerMock.ResetCalls();

                DataBridge.BridgeData(packNode.GUID.ToString(), new ArrayList { typeDefinitionString, "someValue", "newValue" });

                validationManagerMock.Verify(mock => mock.HandleValidation(It.Is<Dictionary<int, object>>(d => d.Count == 1)), Times.Once());
            }

            [Test]
            public void WithCachedValuesWithEdits_ShouldCallHandleValidationForUnCachedValues()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();
                var validationManagerMock = new Mock<IValidationManager>();
                validationManagerMock.Setup(mock => mock.HandleValidation(It.IsAny<Dictionary<int, object>>()));
                validationManagerMock.Setup(mock => mock.Warnings).Returns(new List<Validation>().AsReadOnly());
                SetValidationManager(packNode, validationManagerMock.Object);
                var typeDefinitionString = "Type Something { prop1: Type1, prop2: Type2 }";

                HookUpTypeDefinition(packNode, typeDefinitionString);

                DataBridge.BridgeData(packNode.GUID.ToString(), new ArrayList { typeDefinitionString, "someValue" });
                validationManagerMock.ResetCalls();

                DataBridge.BridgeData(packNode.GUID.ToString(), new ArrayList { typeDefinitionString, "someNewValue", "newValue" });

                validationManagerMock.Verify(mock => mock.HandleValidation(It.Is<Dictionary<int, object>>(d => d.Count == 2)), Times.Once());
            }

            [Test]
            public void WarningStateChange_ShouldCallOnNodeModified()
            {
                var onNodeModifiedCalled = false;
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();
                packNode.Modified += (x) => onNodeModifiedCalled = true;

                var validationManagerMock = new Mock<IValidationManager>();
                validationManagerMock.Setup(mock => mock.HandleValidation(It.IsAny<Dictionary<int, object>>()));
                validationManagerMock.SetupSequence(mock => mock.Warnings)
                    .Returns(new List<Validation>().AsReadOnly())
                    .Returns(new List<Validation>() { new Validation() }.AsReadOnly());

                SetValidationManager(packNode, validationManagerMock.Object);

                HookUpTypeDefinition(packNode, typeDefinitionString);

                Assert.True(onNodeModifiedCalled);
            }
        }

        public class BuildOutPutAstMethod : PackTests
        {
            [Test]
            public void NullNodeList_ReturnsNullNode()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();
                packNode.InPorts[0].Connectors.Add(new Graph.Connectors.ConnectorModel(new PortModel(PortType.Input, packNode, new PortData(null, null)), new PortModel(PortType.Input, packNode, new PortData(null, null)), Guid.NewGuid()));

                HookUpTypeDefinition(packNode);

                packNode.State = ElementState.Active;

                var output = packNode.BuildOutputAst(null);

                Assert.That(output.ElementAt(1), Is.InstanceOf<BinaryExpressionNode>());
                var binaryOutput = output.ElementAt(1) as BinaryExpressionNode;
                Assert.That(binaryOutput.RightNode, Is.InstanceOf<NullNode>());
            }

            [Test]
            public void NullNodeInsideNodeList_ReturnsNullNode()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();
                packNode.InPorts[0].Connectors.Add(new Graph.Connectors.ConnectorModel(new PortModel(PortType.Input, packNode, new PortData(null, null)), new PortModel(PortType.Input, packNode, new PortData(null, null)), Guid.NewGuid()));

                HookUpTypeDefinition(packNode);

                packNode.State = ElementState.Active;

                var output = packNode.BuildOutputAst(new List<AssociativeNode> { AstFactory.BuildStringNode("DST"), AstFactory.BuildNullNode() });

                Assert.That(output.ElementAt(1), Is.InstanceOf<BinaryExpressionNode>());
                var binaryOutput = output.ElementAt(1) as BinaryExpressionNode;
                Assert.That(binaryOutput.RightNode, Is.InstanceOf<NullNode>());
            }

            [Test]
            public void NodeInWarningState_ReturnsNullNode()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();
                packNode.InPorts[0].Connectors.Add(new Graph.Connectors.ConnectorModel(new PortModel(PortType.Input, packNode, new PortData(null, null)), new PortModel(PortType.Input, packNode, new PortData(null, null)), Guid.NewGuid()));

                HookUpTypeDefinition(packNode);

                packNode.State = ElementState.Warning;

                var output = packNode.BuildOutputAst(new List<AssociativeNode> { AstFactory.BuildStringNode("DST"), AstFactory.BuildStringNode("a") });

                Assert.That(output.ElementAt(1), Is.InstanceOf<BinaryExpressionNode>());
                var binaryOutput = output.ElementAt(1) as BinaryExpressionNode;
                Assert.That(binaryOutput.RightNode, Is.InstanceOf<NullNode>());
            }

            [Test]
            public void ValidDateAndInputs_ReturnFunctionCallNode()
            {
                OpenModel(testFileWithPackUnPackNodes);

                var packNode = GetPackNode();
                packNode.InPorts[0].Connectors.Add(new Graph.Connectors.ConnectorModel(new PortModel(PortType.Input, packNode, new PortData(null, null)), new PortModel(PortType.Input, packNode, new PortData(null, null)), Guid.NewGuid()));

                HookUpTypeDefinition(packNode);

                packNode.State = ElementState.Active;

                var output = packNode.BuildOutputAst(new List<AssociativeNode> { AstFactory.BuildStringNode("DST"), AstFactory.BuildStringNode("a") });

                Assert.That(output.ElementAt(1), Is.InstanceOf<BinaryExpressionNode>());
                var binaryOutput = output.ElementAt(1) as BinaryExpressionNode;

                Assert.That(binaryOutput.RightNode, Is.InstanceOf<IdentifierListNode>());
                var identifierListNode = binaryOutput.RightNode as IdentifierListNode;

                Assert.That(identifierListNode.RightNode, Is.InstanceOf<FunctionCallNode>());
            }
        }
    }
}
