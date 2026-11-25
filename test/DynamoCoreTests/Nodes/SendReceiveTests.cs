using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests.Nodes
{
    [TestFixture]
    internal class SendReceiveTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private static ConnectorModel Connect(NodeModel start, int startIndex, NodeModel end, int endIndex)
        {
            return ConnectorModel.Make(start, end, startIndex, endIndex);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSendAndReceiveShareChannelThenDataFlowsWithoutPhysicalWire()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var number = new DoubleInput { Value = "42" };
            var send = new Send { ChannelName = "Height" };
            var receive = new Receive { ChannelName = "Height" };
            var multiply = new CodeBlockNodeModel("x * 2;", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(number, false);
            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(receive, false);
            workspace.AddAndRegisterNode(multiply, false);

            Connect(number, 0, send, 0);
            Connect(receive, 0, multiply, 0);

            RunCurrentModel();

            Assert.IsFalse(workspace.Connectors.Any(c =>
                ReferenceEquals(c.Start.Owner, send) && ReferenceEquals(c.End.Owner, receive)));
            Assert.AreEqual(1, receive.BoundSenders.Count);
            Assert.That(receive.BoundSenders[0], Is.SameAs(send));
            Assert.AreEqual(DataChannelBindingStatus.Bound, send.ChannelStatus);
            Assert.AreEqual(DataChannelBindingStatus.Bound, receive.ChannelStatus);
            AssertPreviewValue(send.GUID.ToString(), 42.0);
            AssertPreviewValue(receive.GUID.ToString(), 42.0);
            AssertPreviewValue(multiply.GUID.ToString(), 84.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSingleSendMatchesMultipleReceivesThenAllReceivesGetValue()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var number = new DoubleInput { Value = "10" };
            var send = new Send { ChannelName = "Shared" };
            var receiveA = new Receive { ChannelName = "Shared" };
            var receiveB = new Receive { ChannelName = "Shared" };
            var plusOne = new CodeBlockNodeModel("x + 1;", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);
            var timesThree = new CodeBlockNodeModel("x * 3;", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(number, false);
            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(receiveA, false);
            workspace.AddAndRegisterNode(receiveB, false);
            workspace.AddAndRegisterNode(plusOne, false);
            workspace.AddAndRegisterNode(timesThree, false);

            Connect(number, 0, send, 0);
            Connect(receiveA, 0, plusOne, 0);
            Connect(receiveB, 0, timesThree, 0);

            RunCurrentModel();

            Assert.That(receiveA.BoundSenders, Has.Count.EqualTo(1));
            Assert.That(receiveA.BoundSenders[0], Is.SameAs(send));
            Assert.That(receiveB.BoundSenders, Has.Count.EqualTo(1));
            Assert.That(receiveB.BoundSenders[0], Is.SameAs(send));
            AssertPreviewValue(plusOne.GUID.ToString(), 11.0);
            AssertPreviewValue(timesThree.GUID.ToString(), 30.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenWildcardMatchesMultipleSendsThenGathersValuesIntoOrderedList()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var numPoints = new DoubleInput { Value = "20" };
            var numPlane = new DoubleInput { Value = "10" };
            var numOther = new DoubleInput { Value = "99" };
            var sendPoints = new Send { ChannelName = "Base Points" };
            var sendPlane = new Send { ChannelName = "Base Plane" };
            var sendOther = new Send { ChannelName = "Other" };
            var receive = new Receive { ChannelName = "Base*" };

            workspace.AddAndRegisterNode(numPoints, false);
            workspace.AddAndRegisterNode(numPlane, false);
            workspace.AddAndRegisterNode(numOther, false);
            workspace.AddAndRegisterNode(sendPoints, false);
            workspace.AddAndRegisterNode(sendPlane, false);
            workspace.AddAndRegisterNode(sendOther, false);
            workspace.AddAndRegisterNode(receive, false);

            Connect(numPoints, 0, sendPoints, 0);
            Connect(numPlane, 0, sendPlane, 0);
            Connect(numOther, 0, sendOther, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.Bound, receive.ChannelStatus);
            Assert.AreEqual(2, receive.BoundSenders.Count);
            // Deterministically sorted by channel key: "Base Plane" before "Base Points"
            Assert.That(receive.BoundSenders[0], Is.SameAs(sendPlane));
            Assert.That(receive.BoundSenders[1], Is.SameAs(sendPoints));
            AssertPreviewValue(receive.GUID.ToString(), new object[] { 10.0, 20.0 });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenWildcardPositionOrCaseDoesNotMatchThenShowsMissingSenderWarning()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var num = new DoubleInput { Value = "5" };
            var send = new Send { ChannelName = "Base Plane" };
            var wrongPosition = new Receive { ChannelName = "*Base" };
            var wrongCase = new Receive { ChannelName = "base*" };
            var singleChar = new Receive { ChannelName = "Base Plan?" };
            var contains = new Receive { ChannelName = "*Plan*" };

            workspace.AddAndRegisterNode(num, false);
            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(wrongPosition, false);
            workspace.AddAndRegisterNode(wrongCase, false);
            workspace.AddAndRegisterNode(singleChar, false);
            workspace.AddAndRegisterNode(contains, false);

            Connect(num, 0, send, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.MissingSender, wrongPosition.ChannelStatus);
            Assert.AreEqual(ElementState.PersistentWarning, wrongPosition.State);
            AssertPreviewValue(wrongPosition.GUID.ToString(), null);

            Assert.AreEqual(DataChannelBindingStatus.MissingSender, wrongCase.ChannelStatus);
            Assert.AreEqual(ElementState.PersistentWarning, wrongCase.State);
            AssertPreviewValue(wrongCase.GUID.ToString(), null);

            Assert.AreEqual(DataChannelBindingStatus.Bound, singleChar.ChannelStatus);
            AssertPreviewValue(singleChar.GUID.ToString(), 5.0);

            Assert.AreEqual(DataChannelBindingStatus.Bound, contains.ChannelStatus);
            AssertPreviewValue(contains.GUID.ToString(), 5.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenStarWildcardIsUsedThenExcludesUnnamedSendNodes()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var numNamed = new DoubleInput { Value = "7" };
            var numUnnamed = new DoubleInput { Value = "999" };
            var sendNamed = new Send { ChannelName = "Alpha" };
            var sendUnnamed = new Send { ChannelName = "" };
            var receiveAll = new Receive { ChannelName = "*" };

            workspace.AddAndRegisterNode(numNamed, false);
            workspace.AddAndRegisterNode(numUnnamed, false);
            workspace.AddAndRegisterNode(sendNamed, false);
            workspace.AddAndRegisterNode(sendUnnamed, false);
            workspace.AddAndRegisterNode(receiveAll, false);

            Connect(numNamed, 0, sendNamed, 0);
            Connect(numUnnamed, 0, sendUnnamed, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.Unassigned, sendUnnamed.ChannelStatus);
            Assert.AreEqual(1, receiveAll.BoundSenders.Count);
            Assert.That(receiveAll.BoundSenders[0], Is.SameAs(sendNamed));
            AssertPreviewValue(receiveAll.GUID.ToString(), 7.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenMultipleSendsShareExactKeyThenReceiveGathersAllIntoList()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var num1 = new DoubleInput { Value = "11" };
            var num2 = new DoubleInput { Value = "22" };
            var send1 = new Send { ChannelName = "Wall" };
            var send2 = new Send { ChannelName = "Wall" };
            var receive = new Receive { ChannelName = "Wall" };

            workspace.AddAndRegisterNode(num1, false);
            workspace.AddAndRegisterNode(num2, false);
            workspace.AddAndRegisterNode(send1, false);
            workspace.AddAndRegisterNode(send2, false);
            workspace.AddAndRegisterNode(receive, false);

            Connect(num1, 0, send1, 0);
            Connect(num2, 0, send2, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.Bound, send1.ChannelStatus);
            Assert.AreEqual(DataChannelBindingStatus.Bound, send2.ChannelStatus);
            Assert.AreEqual(DataChannelBindingStatus.Bound, receive.ChannelStatus);
            Assert.AreEqual(2, receive.BoundSenders.Count);

            var expectedOrder = send1.GUID.CompareTo(send2.GUID) <= 0
                ? new object[] { 11.0, 22.0 }
                : new object[] { 22.0, 11.0 };
            AssertPreviewValue(receive.GUID.ToString(), expectedOrder);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenInputUpstreamOfAnyMatchedSendChangesThenWildcardReceiveReEvaluatesInDeltaExecution()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var inputA = new DoubleInput { Value = "10", IsSetAsInput = true };
            var inputB = new DoubleInput { Value = "20", IsSetAsInput = true };
            var sendA = new Send { ChannelName = "Dim_A" };
            var sendB = new Send { ChannelName = "Dim_B" };
            var receive = new Receive { ChannelName = "Dim_*" };
            var sum = new CodeBlockNodeModel("x[0] + x[1];", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(inputA, false);
            workspace.AddAndRegisterNode(inputB, false);
            workspace.AddAndRegisterNode(sendA, false);
            workspace.AddAndRegisterNode(sendB, false);
            workspace.AddAndRegisterNode(receive, false);
            workspace.AddAndRegisterNode(sum, false);

            Connect(inputA, 0, sendA, 0);
            Connect(inputB, 0, sendB, 0);
            Connect(receive, 0, sum, 0);

            RunCurrentModel();
            AssertPreviewValue(sum.GUID.ToString(), 30.0);

            // Simulate Dynamo Player / Generative Design updating first input
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                workspace.Guid, inputA.GUID, nameof(DoubleInput.Value), "25"));
            RunCurrentModel();
            AssertPreviewValue(sum.GUID.ToString(), 45.0);

            // Update second input
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                workspace.Guid, inputB.GUID, nameof(DoubleInput.Value), "100"));
            RunCurrentModel();
            AssertPreviewValue(sum.GUID.ToString(), 125.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenCheckingPlayerMetadataThenSendAndReceiveAreNotInputNodes()
        {
            var send = new Send { ChannelName = "Metric", IsSetAsInput = true };
            var receive = new Receive { ChannelName = "Metric*", IsSetAsInput = true };

            Assert.IsFalse(send.IsInputNode);
            Assert.IsFalse(receive.IsInputNode);
            Assert.IsFalse(send.IsSetAsInput);
            Assert.IsFalse(receive.IsSetAsInput);
            Assert.IsTrue(send.IsOutputNode);
            Assert.IsTrue(receive.IsOutputNode);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenGraphWithWildcardSendReceiveIsSavedAndReopenedThenEvaluatesHeadlessly()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var inputA = new DoubleInput { Value = "7", IsSetAsInput = true };
            var inputB = new DoubleInput { Value = "3", IsSetAsInput = true };
            var sendA = new Send { ChannelName = "Radius_1" };
            var sendB = new Send { ChannelName = "Radius_2" };
            var receive = new Receive { ChannelName = "Radius_*", IsSetAsOutput = true };
            var sum = new CodeBlockNodeModel("r[0] + r[1];", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(inputA, false);
            workspace.AddAndRegisterNode(inputB, false);
            workspace.AddAndRegisterNode(sendA, false);
            workspace.AddAndRegisterNode(sendB, false);
            workspace.AddAndRegisterNode(receive, false);
            workspace.AddAndRegisterNode(sum, false);

            Connect(inputA, 0, sendA, 0);
            Connect(inputB, 0, sendB, 0);
            Connect(receive, 0, sum, 0);

            RunCurrentModel();
            AssertPreviewValue(sum.GUID.ToString(), 10.0);

            string tempFile = Path.Combine(Path.GetTempPath(), $"SendReceiveTest_{Guid.NewGuid():N}.dyn");
            try
            {
                workspace.Save(tempFile, false, CurrentDynamoModel.EngineController);

                OpenModel(tempFile);
                RunCurrentModel();

                var reopenedWorkspace = CurrentDynamoModel.CurrentWorkspace;
                var reopenedReceive = reopenedWorkspace.Nodes.OfType<Receive>().Single();
                var reopenedSum = reopenedWorkspace.Nodes.OfType<CodeBlockNodeModel>().Single();
                var reopenedSendA = reopenedWorkspace.Nodes.OfType<Send>().Single(s => s.ChannelName == "Radius_1");
                var reopenedInputA = reopenedSendA.InPorts[0].Connectors[0].Start.Owner;

                Assert.AreEqual("Radius_*", reopenedReceive.ChannelName);
                Assert.AreEqual(2, reopenedReceive.BoundSenders.Count);
                AssertPreviewValue(reopenedReceive.GUID.ToString(), new object[] { 7.0, 3.0 });
                AssertPreviewValue(reopenedSum.GUID.ToString(), 10.0);

                // Verify input mutation works after headless reopen (Player / GD flow)
                CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                    reopenedWorkspace.Guid, reopenedInputA.GUID, nameof(DoubleInput.Value), "12"));
                RunCurrentModel();
                AssertPreviewValue(reopenedReceive.GUID.ToString(), new object[] { 12.0, 3.0 });
                AssertPreviewValue(reopenedSum.GUID.ToString(), 15.0);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenMatchedSendsAreRemovedAndRestoredThenReceiveTransitionsCleanly()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var num1 = new DoubleInput { Value = "10" };
            var num2 = new DoubleInput { Value = "20" };
            var send1 = new Send { ChannelName = "Temp_1" };
            var send2 = new Send { ChannelName = "Temp_2" };
            var receive = new Receive { ChannelName = "Temp_*" };

            workspace.AddAndRegisterNode(num1, false);
            workspace.AddAndRegisterNode(num2, false);
            workspace.AddAndRegisterNode(send1, false);
            workspace.AddAndRegisterNode(send2, false);
            workspace.AddAndRegisterNode(receive, false);
            Connect(num1, 0, send1, 0);
            Connect(num2, 0, send2, 0);

            RunCurrentModel();
            AssertPreviewValue(receive.GUID.ToString(), new object[] { 10.0, 20.0 });

            // Removing one of two matched senders unwraps to single value
            workspace.RemoveAndDisposeNode(send2);
            RunCurrentModel();
            Assert.AreEqual(1, receive.BoundSenders.Count);
            AssertPreviewValue(receive.GUID.ToString(), 10.0);

            // Removing the last matched sender triggers MissingSender warning and null output
            workspace.RemoveAndDisposeNode(send1);
            RunCurrentModel();
            Assert.AreEqual(DataChannelBindingStatus.MissingSender, receive.ChannelStatus);
            Assert.AreEqual(ElementState.PersistentWarning, receive.State);
            Assert.AreEqual(0, receive.BoundSenders.Count);
            AssertPreviewValue(receive.GUID.ToString(), null);

            // Adding a new matching Send rebinds and clears warning
            var num3 = new DoubleInput { Value = "123" };
            var send3 = new Send { ChannelName = "Temp_3" };
            workspace.AddAndRegisterNode(num3, false);
            workspace.AddAndRegisterNode(send3, false);
            Connect(num3, 0, send3, 0);

            RunCurrentModel();
            Assert.AreEqual(DataChannelBindingStatus.Bound, receive.ChannelStatus);
            Assert.That(receive.State, Is.Not.EqualTo(ElementState.PersistentWarning));
            Assert.That(receive.BoundSenders[0], Is.SameAs(send3));
            AssertPreviewValue(receive.GUID.ToString(), 123.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSendDependsOnMatchingWildcardReceiveThenCycleIsDetectedAndRecovered()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var send = new Send { ChannelName = "Loop_1" };
            var receive = new Receive { ChannelName = "Loop_*" };
            var inc = new CodeBlockNodeModel("x + 1;", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(receive, false);
            workspace.AddAndRegisterNode(inc, false);

            Connect(receive, 0, inc, 0);
            var c2 = Connect(inc, 0, send, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.CyclicDependency, receive.ChannelStatus);
            Assert.AreEqual(ElementState.PersistentWarning, receive.State);
            Assert.AreEqual(0, receive.BoundSenders.Count);

            // Break the cycle and feed a normal input into Send
            c2.Delete();
            var num = new DoubleInput { Value = "5" };
            workspace.AddAndRegisterNode(num, false);
            Connect(num, 0, send, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.Bound, receive.ChannelStatus);
            Assert.That(receive.State, Is.Not.EqualTo(ElementState.PersistentWarning));
            Assert.That(receive.BoundSenders[0], Is.SameAs(send));
            AssertPreviewValue(receive.GUID.ToString(), 5.0);
            AssertPreviewValue(inc.GUID.ToString(), 6.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenMultiHopCycleAcrossTwoChannelsOccursThenCycleIsDetected()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var sendA = new Send { ChannelName = "A" };
            var receiveA = new Receive { ChannelName = "A" };
            var sendB = new Send { ChannelName = "B" };
            var receiveB = new Receive { ChannelName = "B" };

            workspace.AddAndRegisterNode(sendA, false);
            workspace.AddAndRegisterNode(receiveA, false);
            workspace.AddAndRegisterNode(sendB, false);
            workspace.AddAndRegisterNode(receiveB, false);

            Connect(receiveA, 0, sendB, 0);
            Connect(receiveB, 0, sendA, 0);

            Assert.DoesNotThrow(() => RunCurrentModel());
            Assert.IsTrue(
                receiveA.ChannelStatus == DataChannelBindingStatus.CyclicDependency ||
                receiveB.ChannelStatus == DataChannelBindingStatus.CyclicDependency);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenAnyMatchedSendIsFrozenThenWildcardReceiveFreezesAndUnfreezesCleanly()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var inputA = new DoubleInput { Value = "10" };
            var inputB = new DoubleInput { Value = "5" };
            var sendA = new Send { ChannelName = "Freeze_A" };
            var sendB = new Send { ChannelName = "Freeze_B" };
            var receive = new Receive { ChannelName = "Freeze_*" };
            var calc = new CodeBlockNodeModel("x[0] + x[1];", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(inputA, false);
            workspace.AddAndRegisterNode(inputB, false);
            workspace.AddAndRegisterNode(sendA, false);
            workspace.AddAndRegisterNode(sendB, false);
            workspace.AddAndRegisterNode(receive, false);
            workspace.AddAndRegisterNode(calc, false);

            Connect(inputA, 0, sendA, 0);
            Connect(inputB, 0, sendB, 0);
            Connect(receive, 0, calc, 0);

            RunCurrentModel();
            AssertPreviewValue(calc.GUID.ToString(), 15.0);

            sendB.IsFrozen = true;
            Assert.IsTrue(receive.IsFrozen);
            Assert.IsTrue(calc.IsFrozen);

            sendB.IsFrozen = false;
            Assert.IsFalse(receive.IsFrozen);
            Assert.IsFalse(calc.IsFrozen);

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                workspace.Guid, inputB.GUID, nameof(DoubleInput.Value), "20"));
            RunCurrentModel();

            AssertPreviewValue(calc.GUID.ToString(), 30.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSendChannelsChangeThenReceiveAvailableChannelsUpdatesSorted()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var sendB = new Send { ChannelName = "Beta" };
            var sendA = new Send { ChannelName = "Alpha" };
            var receive = new Receive();

            workspace.AddAndRegisterNode(sendB, false);
            workspace.AddAndRegisterNode(sendA, false);
            workspace.AddAndRegisterNode(receive, false);

            CollectionAssert.AreEqual(new[] { "Alpha", "Beta" }, receive.AvailableChannels);

            sendB.ChannelName = "Gamma";
            CollectionAssert.AreEqual(new[] { "Alpha", "Gamma" }, receive.AvailableChannels);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenUndoRedoChangesPatternThenBindingRestores()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var input = new DoubleInput { Value = "33" };
            var send = new Send { ChannelName = "Alpha_1" };
            var receive = new Receive { ChannelName = "Alpha_*" };

            workspace.AddAndRegisterNode(input, false);
            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(receive, false);
            Connect(input, 0, send, 0);

            RunCurrentModel();
            AssertPreviewValue(receive.GUID.ToString(), 33.0);

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                workspace.Guid, receive.GUID, nameof(Receive.ChannelName), "Beta_*"));
            RunCurrentModel();
            Assert.AreEqual("Beta_*", receive.ChannelName);
            Assert.AreEqual(0, receive.BoundSenders.Count);
            AssertPreviewValue(receive.GUID.ToString(), null);

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UndoRedoCommand(
                DynamoModel.UndoRedoCommand.Operation.Undo));
            RunCurrentModel();
            Assert.AreEqual("Alpha_*", receive.ChannelName);
            Assert.That(receive.BoundSenders[0], Is.SameAs(send));
            AssertPreviewValue(receive.GUID.ToString(), 33.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenWildcardSendAndReceiveAreInsideCustomNodeThenScopedToCustomNode()
        {
            var customNodeId = Guid.NewGuid();
            var customWorkspace = CurrentDynamoModel.CustomNodeManager.CreateCustomNode(
                "SendReceiveCustomNode",
                "TestCategory",
                "Tests workspace-scoped Send and Receive with wildcards inside a custom node.",
                customNodeId);

            var symbol = new Symbol { InputSymbol = "x" };
            var customSend = new Send { ChannelName = "Internal_1" };
            var customReceive = new Receive { ChannelName = "*" };
            var multiply = new CodeBlockNodeModel("v * 10;", 0, 0, CurrentDynamoModel.LibraryServices, customWorkspace.ElementResolver);
            var outputNode = new Output { Symbol = "result" };

            customWorkspace.AddAndRegisterNode(symbol, false);
            customWorkspace.AddAndRegisterNode(customSend, false);
            customWorkspace.AddAndRegisterNode(customReceive, false);
            customWorkspace.AddAndRegisterNode(multiply, false);
            customWorkspace.AddAndRegisterNode(outputNode, false);

            Connect(symbol, 0, customSend, 0);
            Connect(customReceive, 0, multiply, 0);
            Connect(multiply, 0, outputNode, 0);

            // Also place a Send in HomeWorkspace to ensure Receive("*") inside the custom node stays isolated
            var homeWorkspace = CurrentDynamoModel.CurrentWorkspace;
            var homeNum = new DoubleInput { Value = "999" };
            var homeSend = new Send { ChannelName = "HomeChannel" };
            var customArg = new DoubleInput { Value = "4" };
            var customInstance = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(customNodeId);

            homeWorkspace.AddAndRegisterNode(homeNum, false);
            homeWorkspace.AddAndRegisterNode(homeSend, false);
            homeWorkspace.AddAndRegisterNode(customArg, false);
            homeWorkspace.AddAndRegisterNode(customInstance, false);

            Connect(homeNum, 0, homeSend, 0);
            Connect(customArg, 0, customInstance, 0);

            RunCurrentModel();

            Assert.AreEqual(1, customReceive.BoundSenders.Count);
            Assert.That(customReceive.BoundSenders[0], Is.SameAs(customSend));
            AssertPreviewValue(customInstance.GUID.ToString(), 40.0);
        }
    }
}
