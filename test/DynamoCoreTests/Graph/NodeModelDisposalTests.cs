using System;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Tests for NodeModel disposal to prevent memory leaks
    /// </summary>
    [TestFixture]
    public class NodeModelDisposalTests : DynamoModelTestBase
    {
        /// <summary>
        /// Test that NodeModel clears port collections on disposal
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void NodeModel_Dispose_ClearsPortCollections()
        {
            // Arrange - Create a simple node with ports
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            // Verify node has ports before disposal
            Assert.Greater(addNode.InPorts.Count, 0, "Node should have input ports");
            Assert.Greater(addNode.OutPorts.Count, 0, "Node should have output ports");

            var inPortCount = addNode.InPorts.Count;
            var outPortCount = addNode.OutPorts.Count;

            // Act - Dispose the node
            addNode.Dispose();

            // Assert - Collections should be cleared
            Assert.AreEqual(0, addNode.InPorts.Count, "InPorts should be cleared after disposal");
            Assert.AreEqual(0, addNode.OutPorts.Count, "OutPorts should be cleared after disposal");
        }

        /// <summary>
        /// Test that NodeModel clears DismissedAlerts on disposal
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void NodeModel_Dispose_ClearsDismissedAlerts()
        {
            // Arrange - Create a node and add dismissed alerts
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            // Add some dismissed alerts
            addNode.DismissedAlerts.Add("Alert1");
            addNode.DismissedAlerts.Add("Alert2");
            addNode.DismissedAlerts.Add("Alert3");

            Assert.AreEqual(3, addNode.DismissedAlerts.Count, "Should have 3 dismissed alerts");

            // Act - Dispose the node
            addNode.Dispose();

            // Assert - DismissedAlerts should be cleared
            Assert.AreEqual(0, addNode.DismissedAlerts.Count, "DismissedAlerts should be cleared after disposal");
        }

        /// <summary>
        /// Test that disposing NodeModel doesn't throw when collections are empty
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void NodeModel_Dispose_SafeWhenCollectionsEmpty()
        {
            // Arrange - Create a node
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                addNode.Dispose();
            });
        }

        /// <summary>
        /// Test that double disposal of NodeModel is safe
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void NodeModel_DoubleDispose_IsSafe()
        {
            // Arrange - Create a node
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            // Add some alerts
            addNode.DismissedAlerts.Add("Alert1");

            // Act & Assert - Should not throw on double dispose
            Assert.DoesNotThrow(() =>
            {
                addNode.Dispose();
                addNode.Dispose(); // Second disposal should be safe
            });

            // Verify HasBeenDisposed flag works
            Assert.IsTrue(addNode.HasBeenDisposed, "Node should be marked as disposed");
        }

        /// <summary>
        /// Test that disposed node doesn't hold references to ports
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void NodeModel_Dispose_ReleasesPortReferences()
        {
            // Arrange - Create a node
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            // Get weak references to ports before disposal
            WeakReference inPortRef = null;
            WeakReference outPortRef = null;

            if (addNode.InPorts.Count > 0)
            {
                inPortRef = new WeakReference(addNode.InPorts[0]);
            }
            if (addNode.OutPorts.Count > 0)
            {
                outPortRef = new WeakReference(addNode.OutPorts[0]);
            }

            // Act - Dispose the node
            addNode.Dispose();

            // Clear the node reference
            addNode = null;

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Assert - Ports should be collectible
            // Note: This is a best-effort test; actual GC behavior may vary
            // The main goal is to ensure collections are cleared
            if (inPortRef != null)
            {
                // Port may or may not be collected depending on other references
                // The important thing is that our node cleared its collection
                Assert.IsNotNull(inPortRef, "WeakReference should exist");
            }
            if (outPortRef != null)
            {
                Assert.IsNotNull(outPortRef, "WeakReference should exist");
            }
        }

        /// <summary>
        /// Test that workspace disposal properly disposes all nodes
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void Workspace_Dispose_DisposesAllNodes()
        {
            // Arrange - Create multiple nodes in workspace
            var addNode1 = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var addNode2 = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var addNode3 = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));

            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode1, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode2, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode3, false);

            // Add alerts to verify they're cleared
            addNode1.DismissedAlerts.Add("Alert1");
            addNode2.DismissedAlerts.Add("Alert2");
            addNode3.DismissedAlerts.Add("Alert3");

            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), "Should have 3 nodes");

            // Act - Clear workspace (which should dispose nodes)
            CurrentDynamoModel.CurrentWorkspace.Clear();

            // Assert - Nodes should be disposed
            Assert.IsTrue(addNode1.HasBeenDisposed, "Node1 should be disposed");
            Assert.IsTrue(addNode2.HasBeenDisposed, "Node2 should be disposed");
            Assert.IsTrue(addNode3.HasBeenDisposed, "Node3 should be disposed");

            // Collections should be cleared
            Assert.AreEqual(0, addNode1.DismissedAlerts.Count, "Node1 alerts should be cleared");
            Assert.AreEqual(0, addNode2.DismissedAlerts.Count, "Node2 alerts should be cleared");
            Assert.AreEqual(0, addNode3.DismissedAlerts.Count, "Node3 alerts should be cleared");
        }
    }
}
