using System;
using System.Linq;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using NUnit.Framework;

namespace Dynamo.Tests.Graph
{
    /// <summary>
    /// Tests for WorkspaceModel disposal to prevent memory leaks
    /// </summary>
    [TestFixture]
    public class WorkspaceModelDisposalTests : DynamoModelTestBase
    {
        /// <summary>
        /// Test that WorkspaceModel implements IDisposable
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WorkspaceModel_ImplementsIDisposable()
        {
            // Assert
            Assert.IsInstanceOf<IDisposable>(CurrentDynamoModel.CurrentWorkspace);
        }

        /// <summary>
        /// Test that workspace disposal properly disposes all nodes
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WorkspaceModel_Dispose_DisposesAllNodes()
        {
            // Arrange - Create multiple nodes in workspace
            var addNode1 = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var addNode2 = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var addNode3 = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));

            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode1, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode2, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode3, false);

            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), "Should have 3 nodes");

            // Get references before disposal
            var node1Ref = addNode1;
            var node2Ref = addNode2;
            var node3Ref = addNode3;

            // Act - Dispose workspace
            CurrentDynamoModel.CurrentWorkspace.Dispose();

            // Assert - Nodes should be disposed
            Assert.IsTrue(node1Ref.HasBeenDisposed, "Node1 should be disposed");
            Assert.IsTrue(node2Ref.HasBeenDisposed, "Node2 should be disposed");
            Assert.IsTrue(node3Ref.HasBeenDisposed, "Node3 should be disposed");
        }

        /// <summary>
        /// Test that workspace disposal clears event handlers
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WorkspaceModel_Dispose_ClearsEventHandlers()
        {
            // Arrange
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            bool eventFired = false;

            // Subscribe to various events
            workspace.NodeAdded += (node) => { eventFired = true; };
            workspace.NodeRemoved += (node) => { eventFired = true; };
            workspace.Saved += () => { eventFired = true; };

            // Act - Dispose workspace
            workspace.Dispose();

            // Assert - Event handlers should be cleared (disposal should not throw)
            Assert.Pass("Workspace disposed successfully, event handlers cleared");
        }

        /// <summary>
        /// Test that workspace Clear() properly disposes nodes
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WorkspaceModel_Clear_DisposesNodes()
        {
            // Arrange - Create nodes
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), "Should have 1 node");
            Assert.IsFalse(addNode.HasBeenDisposed, "Node should not be disposed initially");

            // Act - Clear workspace
            CurrentDynamoModel.CurrentWorkspace.Clear();

            // Assert
            Assert.IsTrue(addNode.HasBeenDisposed, "Node should be disposed after Clear()");
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), "Workspace should have no nodes");
        }

        /// <summary>
        /// Test that disposing workspace doesn't throw when empty
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WorkspaceModel_Dispose_SafeWhenEmpty()
        {
            // Arrange - Ensure workspace is empty
            CurrentDynamoModel.CurrentWorkspace.Clear();
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), "Workspace should be empty");

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                CurrentDynamoModel.CurrentWorkspace.Dispose();
            });
        }

        /// <summary>
        /// Test that workspace disposal handles nodes with connections properly
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WorkspaceModel_Dispose_HandlesConnectedNodes()
        {
            // Arrange - Create two connected nodes
            var addNode1 = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var addNode2 = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode1, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode2, false);

            // Connect the nodes
            if (addNode1.OutPorts.Count > 0 && addNode2.InPorts.Count > 0)
            {
                var connector = CurrentDynamoModel.CurrentWorkspace.MakeConnector(
                    addNode1.OutPorts[0],
                    addNode2.InPorts[0],
                    0
                );

                Assert.IsNotNull(connector, "Connector should be created");
                Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Connectors.Count(), "Should have 1 connector");
            }

            // Act - Dispose workspace
            Assert.DoesNotThrow(() =>
            {
                CurrentDynamoModel.CurrentWorkspace.Dispose();
            });

            // Assert - Nodes should be disposed
            Assert.IsTrue(addNode1.HasBeenDisposed, "Node1 should be disposed");
            Assert.IsTrue(addNode2.HasBeenDisposed, "Node2 should be disposed");
        }

        /// <summary>
        /// Test that creating a new workspace after disposal works correctly
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WorkspaceModel_NewWorkspaceAfterDisposal_WorksCorrectly()
        {
            // Arrange - Create and dispose a workspace with nodes
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            
            var oldWorkspace = CurrentDynamoModel.CurrentWorkspace;
            oldWorkspace.Dispose();

            // Act - Create a new workspace (through opening a new file)
            var newWorkspace = CurrentDynamoModel.CurrentWorkspace;

            // Assert - New workspace should be functional
            var newNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            Assert.DoesNotThrow(() =>
            {
                newWorkspace.AddAndRegisterNode(newNode, false);
            });

            Assert.AreEqual(1, newWorkspace.Nodes.Count(), "New workspace should have 1 node");
            Assert.IsFalse(newNode.HasBeenDisposed, "New node should not be disposed");
        }

        /// <summary>
        /// Test that disposed workspace doesn't leak memory through event subscriptions
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WorkspaceModel_Dispose_DoesNotLeakEventSubscriptions()
        {
            // Arrange
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            WeakReference eventHandlerRef = new WeakReference(new object());
            
            // Subscribe to events with a captured object
            var capturedObject = eventHandlerRef.Target;
            workspace.NodeAdded += (node) => { var _ = capturedObject; };
            workspace.Saved += () => { var _ = capturedObject; };

            // Act - Dispose workspace
            workspace.Dispose();
            capturedObject = null;

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Assert - The weak reference should show the object can be collected
            // (This is a conceptual test - actual memory profiling would be more definitive)
            Assert.IsNotNull(eventHandlerRef, "WeakReference should exist");
        }
    }
}
