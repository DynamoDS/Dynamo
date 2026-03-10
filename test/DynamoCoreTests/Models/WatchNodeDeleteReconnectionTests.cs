using CoreNodeModels;
using Dynamo.Graph;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Tests.ModelsTest
{
    /// <summary>
    /// This test class will contain test method for executing methods in the DynamoModel class
    /// </summary>
    [TestFixture]
    class WatchNodeDeleteReconnectionTests : DynamoModelTestBase
    {
        private const string InlineWatchFixtureWatchNodeId = "7cea46ef214540cdbe0ee876e7fa7694";
        private const string InlineWatchFixtureUpstreamNodeId = "ba7faaa1a3664a65a43245bd9ca101a4";
        private const string InlineWatchFixtureDownstreamNodeAId = "038e2da4087e486eae17dbafcb02489b";
        private const string InlineWatchFixtureDownstreamNodeBId = "f5f7bb5c2efb459bb67cc252d03c9e95";

        private const string InlineWatchFixtureIncomingConnectorId = "35b91481-ed3b-47a2-bf3a-607001c64a24";
        private const string InlineWatchFixtureDownstreamConnectorAId = "6893a374-9463-4ccc-be79-6cbe19800dce";
        private const string InlineWatchFixtureDownstreamConnectorBId = "ccbd49ec-797d-441a-be32-df772348d4c9";

        private void OpenInlineWatchDeleteFixtureGraph()
        {
            OpenModel(@"core\watch\DeleteInlineWatch_RewireFixture.dyn");
        }

        private HomeWorkspaceModel GetHomeWorkspace()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            Assert.IsNotNull(workspace);
            return workspace;
        }

        private T GetNode<T>(string nodeId) where T : NodeModel
        {
            var nodeGuid = ParseFixtureId(nodeId);
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(n => n.GUID == nodeGuid) as T;
            Assert.IsNotNull(node);
            return node;
        }

        private ConnectorModel GetConnector(string connectorId)
        {
            var connectorGuid = ParseFixtureId(connectorId);
            var connector = CurrentDynamoModel.CurrentWorkspace.Connectors.FirstOrDefault(c => c.GUID == connectorGuid);
            Assert.IsNotNull(connector);
            return connector;
        }

        private static Guid ParseFixtureId(string id)
        {
            return Guid.Parse(id);
        }

        [Test]
        [Category("UnitTests")]
        public void DeleteInlineWatchNode_RewiresAndSuppressesAutoRun()
        {
            // Arrange
            OpenInlineWatchDeleteFixtureGraph();
            var workspace = GetHomeWorkspace();

            workspace.RunSettings.RunType = RunType.Automatic;
            var evaluationCountBeforeDelete = workspace.EvaluationCount;
            var watchNode = GetNode<Watch>(InlineWatchFixtureWatchNodeId);

            // Delete the watch node
            CurrentDynamoModel.DeleteModelInternal(new List<ModelBase> { watchNode });

            // Assert that the watch node has been deleted and that the workspace did not automatically re-run
            Assert.AreEqual(evaluationCountBeforeDelete, workspace.EvaluationCount);
            Assert.IsNull(workspace.Nodes.FirstOrDefault(node => node.GUID == ParseFixtureId(InlineWatchFixtureWatchNodeId)));

            var connectorA = GetConnector(InlineWatchFixtureDownstreamConnectorAId);
            var connectorB = GetConnector(InlineWatchFixtureDownstreamConnectorBId);

            // Assert that the connectors have been rewired to connect
            // the upstream node directly to the downstream nodes
            Assert.AreEqual(ParseFixtureId(InlineWatchFixtureUpstreamNodeId), connectorA.Start.Owner.GUID);
            Assert.AreEqual(ParseFixtureId(InlineWatchFixtureDownstreamNodeAId), connectorA.End.Owner.GUID);
            Assert.AreEqual(ParseFixtureId(InlineWatchFixtureUpstreamNodeId), connectorB.Start.Owner.GUID);
            Assert.AreEqual(ParseFixtureId(InlineWatchFixtureDownstreamNodeBId), connectorB.End.Owner.GUID);
        }

        [Test]
        [Category("UnitTests")]
        public void DeleteInlineWatchNode_RecreatesIncomingPinsOnFirstDownstream()
        {
            // Arrange
            OpenInlineWatchDeleteFixtureGraph();
            var watchNode = GetNode<Watch>(InlineWatchFixtureWatchNodeId);

            var incomingConnector = GetConnector(InlineWatchFixtureIncomingConnectorId);
            incomingConnector.AddPin(new ConnectorPinModel(120.0, 220.0, Guid.NewGuid(), incomingConnector.GUID));
            incomingConnector.AddPin(new ConnectorPinModel(180.0, 260.0, Guid.NewGuid(), incomingConnector.GUID));

            // Delete the watch node
            CurrentDynamoModel.DeleteModelInternal(new List<ModelBase> { watchNode });

            var connectorA = GetConnector(InlineWatchFixtureDownstreamConnectorAId);
            var connectorB = GetConnector(InlineWatchFixtureDownstreamConnectorBId);

            // Assert that the incoming connector's pins have been transferred
            // to the first downstream connector (connectorA) and that connectorB has no pins
            Assert.AreEqual(2, connectorA.ConnectorPinModels.Count);
            Assert.AreEqual(0, connectorB.ConnectorPinModels.Count);
            Assert.IsTrue(connectorA.ConnectorPinModels.All(pin => pin.ConnectorId == connectorA.GUID));
            CollectionAssert.AreEquivalent(
                new[] { (120.0, 220.0), (180.0, 260.0) },
                connectorA.ConnectorPinModels.Select(pin => (pin.Position.X, pin.Position.Y)).ToList());
        }

        [Test]
        [Category("UnitTests")]
        public void UndoDeleteInlineWatchNode_RestoresWatchDataAndConnections()
        {
            // Arrange
            OpenInlineWatchDeleteFixtureGraph();
            var workspace = GetHomeWorkspace();
            workspace.RunSettings.RunType = RunType.Manual;
            BeginRun();

            var evaluationCountAfterRun = workspace.EvaluationCount;
            var watchNode = GetNode<Watch>(InlineWatchFixtureWatchNodeId);

            // Delete the watch node and then undo the deletion
            CurrentDynamoModel.DeleteModelInternal(new List<ModelBase> { watchNode });
            workspace.Undo();

            // Assert that the watch node has been restored with its cached
            // value and that the workspace did not automatically re-run
            Assert.AreEqual(evaluationCountAfterRun, workspace.EvaluationCount);

            var restoredWatch = GetNode<Watch>(InlineWatchFixtureWatchNodeId);

            Assert.IsTrue(restoredWatch.HasRunOnce);
            Assert.IsNotNull(restoredWatch.CachedValue);

            var incomingConnector = GetConnector(InlineWatchFixtureIncomingConnectorId);
            var connectorA = GetConnector(InlineWatchFixtureDownstreamConnectorAId);
            var connectorB = GetConnector(InlineWatchFixtureDownstreamConnectorBId);

            // Assert that the connectors have been restored to connect
            // the upstream node to the watch node and the watch node to the downstream nodes
            Assert.AreEqual(ParseFixtureId(InlineWatchFixtureUpstreamNodeId), incomingConnector.Start.Owner.GUID);
            Assert.AreEqual(ParseFixtureId(InlineWatchFixtureWatchNodeId), incomingConnector.End.Owner.GUID);
            Assert.AreEqual(ParseFixtureId(InlineWatchFixtureWatchNodeId), connectorA.Start.Owner.GUID);
            Assert.AreEqual(ParseFixtureId(InlineWatchFixtureWatchNodeId), connectorB.Start.Owner.GUID);
        }

        [Test]
        [Category("UnitTests")]
        public void DeleteInlineWatchNode_UndoRedo_PreservesPinTransferSemantics()
        {
            // Arrange
            OpenInlineWatchDeleteFixtureGraph();
            var workspace = GetHomeWorkspace();
            workspace.RunSettings.RunType = RunType.Manual;

            var expectedPinCoordinates = new[] { (120.0, 220.0), (180.0, 260.0) };

            var watchNode = GetNode<Watch>(InlineWatchFixtureWatchNodeId);
            var incomingConnector = GetConnector(InlineWatchFixtureIncomingConnectorId);
            incomingConnector.AddPin(new ConnectorPinModel(120.0, 220.0, Guid.NewGuid(), incomingConnector.GUID));
            incomingConnector.AddPin(new ConnectorPinModel(180.0, 260.0, Guid.NewGuid(), incomingConnector.GUID));


            // Delete the watch node, then undo and redo the deletion
            CurrentDynamoModel.DeleteModelInternal(new List<ModelBase> { watchNode });

            // Assert that after deletion, the first downstream connector has the incoming pins
            // and the second downstream connector has no pins
            var connectorAAfterDelete = GetConnector(InlineWatchFixtureDownstreamConnectorAId);
            var connectorBAfterDelete = GetConnector(InlineWatchFixtureDownstreamConnectorBId);
            CollectionAssert.AreEquivalent(
                expectedPinCoordinates,
                connectorAAfterDelete.ConnectorPinModels.Select(pin => (pin.Position.X, pin.Position.Y)).ToList());
            Assert.AreEqual(0, connectorBAfterDelete.ConnectorPinModels.Count);

            // Undo the deletion
            workspace.Undo();

            // Assert that after undoing the deletion, the incoming connector
            // has its pins restored and both downstream connectors have no pins
            var incomingConnectorAfterUndo = GetConnector(InlineWatchFixtureIncomingConnectorId);
            var connectorAAfterUndo = GetConnector(InlineWatchFixtureDownstreamConnectorAId);
            var connectorBAfterUndo = GetConnector(InlineWatchFixtureDownstreamConnectorBId);
            Assert.AreEqual(2, incomingConnectorAfterUndo.ConnectorPinModels.Count);
            Assert.AreEqual(0, connectorAAfterUndo.ConnectorPinModels.Count);
            Assert.AreEqual(0, connectorBAfterUndo.ConnectorPinModels.Count);

            // Redo the deletion
            workspace.Redo();

            // Assert that after redoing the deletion, the first downstream connector has the incoming pins again
            var connectorAAfterRedo = GetConnector(InlineWatchFixtureDownstreamConnectorAId);
            var connectorBAfterRedo = GetConnector(InlineWatchFixtureDownstreamConnectorBId);
            CollectionAssert.AreEquivalent(
                expectedPinCoordinates,
                connectorAAfterRedo.ConnectorPinModels.Select(pin => (pin.Position.X, pin.Position.Y)).ToList());
            Assert.AreEqual(0, connectorBAfterRedo.ConnectorPinModels.Count);
        }
    }
}
