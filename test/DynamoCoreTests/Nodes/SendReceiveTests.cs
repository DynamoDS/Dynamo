using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
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
            Assert.That(receive.BoundSender, Is.SameAs(send));
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

            Assert.That(receiveA.BoundSender, Is.SameAs(send));
            Assert.That(receiveB.BoundSender, Is.SameAs(send));
            AssertPreviewValue(plusOne.GUID.ToString(), 11.0);
            AssertPreviewValue(timesThree.GUID.ToString(), 30.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenInputUpstreamOfSendChangesThenReceiveReEvaluatesInDeltaExecution()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var input = new DoubleInput { Value = "10", IsSetAsInput = true };
            var send = new Send { ChannelName = "Width" };
            var receive = new Receive { ChannelName = "Width" };
            var output = new CodeBlockNodeModel("x + 5;", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(input, false);
            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(receive, false);
            workspace.AddAndRegisterNode(output, false);

            Connect(input, 0, send, 0);
            Connect(receive, 0, output, 0);

            RunCurrentModel();
            AssertPreviewValue(receive.GUID.ToString(), 10.0);
            AssertPreviewValue(output.GUID.ToString(), 15.0);

            // Simulate Dynamo Player / Generative Design updating the input value between runs
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                workspace.Guid, input.GUID, nameof(DoubleInput.Value), "25"));
            RunCurrentModel();
            AssertPreviewValue(receive.GUID.ToString(), 25.0);
            AssertPreviewValue(output.GUID.ToString(), 30.0);

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                workspace.Guid, input.GUID, nameof(DoubleInput.Value), "100"));
            RunCurrentModel();
            AssertPreviewValue(receive.GUID.ToString(), 100.0);
            AssertPreviewValue(output.GUID.ToString(), 105.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenCheckingPlayerMetadataThenSendAndReceiveAreNotInputNodes()
        {
            var send = new Send { ChannelName = "Metric", IsSetAsInput = true };
            var receive = new Receive { ChannelName = "Metric", IsSetAsInput = true };

            Assert.IsFalse(send.IsInputNode);
            Assert.IsFalse(receive.IsInputNode);
            Assert.IsFalse(send.IsSetAsInput);
            Assert.IsFalse(receive.IsSetAsInput);
            Assert.IsTrue(send.IsOutputNode);
            Assert.IsTrue(receive.IsOutputNode);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenGraphWithSendReceiveIsSavedAndReopenedThenEvaluatesHeadlessly()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var input = new DoubleInput { Value = "7", IsSetAsInput = true };
            var send = new Send { ChannelName = "Radius" };
            var receive = new Receive { ChannelName = "Radius", IsSetAsOutput = true };
            var calc = new CodeBlockNodeModel("r * 3;", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(input, false);
            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(receive, false);
            workspace.AddAndRegisterNode(calc, false);

            Connect(input, 0, send, 0);
            Connect(receive, 0, calc, 0);

            RunCurrentModel();
            AssertPreviewValue(calc.GUID.ToString(), 21.0);

            string tempFile = Path.Combine(Path.GetTempPath(), $"SendReceiveTest_{Guid.NewGuid():N}.dyn");
            try
            {
                workspace.Save(tempFile, false, CurrentDynamoModel.EngineController);

                OpenModel(tempFile);
                RunCurrentModel();

                var reopenedWorkspace = CurrentDynamoModel.CurrentWorkspace;
                var reopenedInput = reopenedWorkspace.Nodes.OfType<DoubleInput>().Single();
                var reopenedSend = reopenedWorkspace.Nodes.OfType<Send>().Single();
                var reopenedReceive = reopenedWorkspace.Nodes.OfType<Receive>().Single();
                var reopenedCalc = reopenedWorkspace.Nodes.OfType<CodeBlockNodeModel>().Single();

                Assert.AreEqual("Radius", reopenedSend.ChannelName);
                Assert.AreEqual("Radius", reopenedReceive.ChannelName);
                Assert.That(reopenedReceive.BoundSender, Is.SameAs(reopenedSend));
                AssertPreviewValue(reopenedReceive.GUID.ToString(), 7.0);
                AssertPreviewValue(reopenedCalc.GUID.ToString(), 21.0);

                // Verify input mutation works after headless reopen (Player / GD flow)
                CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                    reopenedWorkspace.Guid, reopenedInput.GUID, nameof(DoubleInput.Value), "12"));
                RunCurrentModel();
                AssertPreviewValue(reopenedReceive.GUID.ToString(), 12.0);
                AssertPreviewValue(reopenedCalc.GUID.ToString(), 36.0);
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
        public void WhenDuplicateSendChannelExistsThenShowsWarningAndResolvesOnRename()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var num1 = new DoubleInput { Value = "11" };
            var num2 = new DoubleInput { Value = "22" };
            var send1 = new Send { ChannelName = "Dup" };
            var send2 = new Send { ChannelName = "Dup" };
            var receive = new Receive { ChannelName = "Dup" };

            workspace.AddAndRegisterNode(num1, false);
            workspace.AddAndRegisterNode(num2, false);
            workspace.AddAndRegisterNode(send1, false);
            workspace.AddAndRegisterNode(send2, false);
            workspace.AddAndRegisterNode(receive, false);

            Connect(num1, 0, send1, 0);
            Connect(num2, 0, send2, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.DuplicateSender, send1.ChannelStatus);
            Assert.AreEqual(DataChannelBindingStatus.DuplicateSender, send2.ChannelStatus);
            Assert.AreEqual(DataChannelBindingStatus.DuplicateSender, receive.ChannelStatus);
            Assert.AreEqual(ElementState.PersistentWarning, send1.State);
            Assert.AreEqual(ElementState.PersistentWarning, send2.State);
            Assert.AreEqual(ElementState.PersistentWarning, receive.State);
            Assert.IsNull(receive.BoundSender);
            AssertPreviewValue(receive.GUID.ToString(), null);

            // Resolve duplicate by renaming send2
            send2.ChannelName = "Other";
            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.Bound, send1.ChannelStatus);
            Assert.AreEqual(DataChannelBindingStatus.Bound, send2.ChannelStatus);
            Assert.AreEqual(DataChannelBindingStatus.Bound, receive.ChannelStatus);
            Assert.That(send1.State, Is.Not.EqualTo(ElementState.PersistentWarning));
            Assert.That(receive.State, Is.Not.EqualTo(ElementState.PersistentWarning));
            Assert.That(receive.BoundSender, Is.SameAs(send1));
            AssertPreviewValue(receive.GUID.ToString(), 11.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSendIsDeletedThenReceiveShowsMissingWarningAndRebindsWhenRestored()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var num1 = new DoubleInput { Value = "99" };
            var send1 = new Send { ChannelName = "Temp" };
            var receive = new Receive { ChannelName = "Temp" };

            workspace.AddAndRegisterNode(num1, false);
            workspace.AddAndRegisterNode(send1, false);
            workspace.AddAndRegisterNode(receive, false);
            Connect(num1, 0, send1, 0);

            RunCurrentModel();
            AssertPreviewValue(receive.GUID.ToString(), 99.0);

            workspace.RemoveAndDisposeNode(send1);
            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.MissingSender, receive.ChannelStatus);
            Assert.AreEqual(ElementState.PersistentWarning, receive.State);
            Assert.IsNull(receive.BoundSender);
            Assert.Contains("Temp", receive.DisplayChannels);
            AssertPreviewValue(receive.GUID.ToString(), null);

            // Add a replacement Send node with the same channel name
            var num2 = new DoubleInput { Value = "123" };
            var send2 = new Send { ChannelName = "Temp" };
            workspace.AddAndRegisterNode(num2, false);
            workspace.AddAndRegisterNode(send2, false);
            Connect(num2, 0, send2, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.Bound, receive.ChannelStatus);
            Assert.That(receive.State, Is.Not.EqualTo(ElementState.PersistentWarning));
            Assert.That(receive.BoundSender, Is.SameAs(send2));
            AssertPreviewValue(receive.GUID.ToString(), 123.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSendDependsOnReceiveOfSameChannelThenCycleIsDetectedAndRecovered()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var send = new Send { ChannelName = "Loop" };
            var receive = new Receive { ChannelName = "Loop" };
            var inc = new CodeBlockNodeModel("x + 1;", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(receive, false);
            workspace.AddAndRegisterNode(inc, false);

            var c1 = Connect(receive, 0, inc, 0);
            var c2 = Connect(inc, 0, send, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.CyclicDependency, receive.ChannelStatus);
            Assert.AreEqual(ElementState.PersistentWarning, receive.State);
            Assert.IsNull(receive.BoundSender);

            // Break the cycle and feed a normal input into Send
            c2.Delete();
            var num = new DoubleInput { Value = "5" };
            workspace.AddAndRegisterNode(num, false);
            Connect(num, 0, send, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.Bound, receive.ChannelStatus);
            Assert.That(receive.State, Is.Not.EqualTo(ElementState.PersistentWarning));
            Assert.That(receive.BoundSender, Is.SameAs(send));
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
        public void WhenSendIsFrozenThenReceiveAndDownstreamFreezeAndUnfreezeCleanly()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var input = new DoubleInput { Value = "10" };
            var send = new Send { ChannelName = "FreezeChannel" };
            var receive = new Receive { ChannelName = "FreezeChannel" };
            var calc = new CodeBlockNodeModel("x + 1;", 0, 0, CurrentDynamoModel.LibraryServices, workspace.ElementResolver);

            workspace.AddAndRegisterNode(input, false);
            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(receive, false);
            workspace.AddAndRegisterNode(calc, false);

            Connect(input, 0, send, 0);
            Connect(receive, 0, calc, 0);

            RunCurrentModel();
            AssertPreviewValue(calc.GUID.ToString(), 11.0);

            send.IsFrozen = true;
            Assert.IsTrue(receive.IsFrozen);
            Assert.IsTrue(calc.IsFrozen);

            send.IsFrozen = false;
            Assert.IsFalse(receive.IsFrozen);
            Assert.IsFalse(calc.IsFrozen);

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                workspace.Guid, input.GUID, nameof(DoubleInput.Value), "20"));
            RunCurrentModel();

            AssertPreviewValue(receive.GUID.ToString(), 20.0);
            AssertPreviewValue(calc.GUID.ToString(), 21.0);
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
        public void WhenUndoRedoChangesChannelNameThenBindingRestores()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var input = new DoubleInput { Value = "33" };
            var send = new Send { ChannelName = "Alpha" };
            var receive = new Receive { ChannelName = "Alpha" };

            workspace.AddAndRegisterNode(input, false);
            workspace.AddAndRegisterNode(send, false);
            workspace.AddAndRegisterNode(receive, false);
            Connect(input, 0, send, 0);

            RunCurrentModel();
            AssertPreviewValue(receive.GUID.ToString(), 33.0);

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                workspace.Guid, send.GUID, nameof(Send.ChannelName), "Beta"));
            RunCurrentModel();
            Assert.AreEqual("Beta", send.ChannelName);
            Assert.IsNull(receive.BoundSender);
            AssertPreviewValue(receive.GUID.ToString(), null);

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UndoRedoCommand(
                DynamoModel.UndoRedoCommand.Operation.Undo));
            RunCurrentModel();
            Assert.AreEqual("Alpha", send.ChannelName);
            Assert.That(receive.BoundSender, Is.SameAs(send));
            AssertPreviewValue(receive.GUID.ToString(), 33.0);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSendAndReceiveAreInsideCustomNodeThenScopedToCustomNode()
        {
            var customNodeId = Guid.NewGuid();
            var customWorkspace = CurrentDynamoModel.CustomNodeManager.CreateCustomNode(
                "SendReceiveCustomNode",
                "TestCategory",
                "Tests workspace-scoped Send and Receive inside a custom node.",
                customNodeId);

            var symbol = new Symbol { InputSymbol = "x" };
            var customSend = new Send { ChannelName = "Internal" };
            var customReceive = new Receive { ChannelName = "Internal" };
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

            // Also place a same-named Send in HomeWorkspace to ensure strict workspace isolation
            var homeWorkspace = CurrentDynamoModel.CurrentWorkspace;
            var homeNum = new DoubleInput { Value = "999" };
            var homeSend = new Send { ChannelName = "Internal" };
            var customArg = new DoubleInput { Value = "4" };
            var customInstance = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(customNodeId);

            homeWorkspace.AddAndRegisterNode(homeNum, false);
            homeWorkspace.AddAndRegisterNode(homeSend, false);
            homeWorkspace.AddAndRegisterNode(customArg, false);
            homeWorkspace.AddAndRegisterNode(customInstance, false);

            Connect(homeNum, 0, homeSend, 0);
            Connect(customArg, 0, customInstance, 0);

            RunCurrentModel();

            Assert.AreEqual(DataChannelBindingStatus.Bound, customSend.ChannelStatus);
            Assert.AreEqual(DataChannelBindingStatus.Bound, homeSend.ChannelStatus);
            Assert.That(customReceive.BoundSender, Is.SameAs(customSend));
            AssertPreviewValue(customInstance.GUID.ToString(), 40.0);
        }
    }
}
