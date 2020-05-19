using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.Selection;
using Dynamo.ViewModels;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class NodeViewTests : DynamoTestUIBase
    {
        // adapted from: http://stackoverflow.com/questions/9336165/correct-method-for-using-the-wpf-dispatcher-in-unit-tests

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();

            DispatcherUtil.DoEvents();
        }

        [Test]
        public void ZIndex_Test_MouseDown()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b8c2a62f-f1ce-4327-8d98-c4e0cc0ebed4");

            // Index of first node == 4.
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);
            nodeView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.PreviewMouseDownEvent,
                Source = this,
            });

            // After click node should have The highest index.
            Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
        }

        [Test]
        public void ZIndex_Test_MouseEnter_Leave()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b8c2a62f-f1ce-4327-8d98-c4e0cc0ebed4");

            // Index of first node == 4.
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);

            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                 {
                     RoutedEvent = Mouse.MouseEnterEvent
                 })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse enter node should have the highest index.
                Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
            };

            dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                {
                    RoutedEvent = Mouse.MouseLeaveEvent
                })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse leave node should have the old index.
                Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
                Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            };
        }

        [Test]
        public void ZIndex_NodeAsMemberOfGroup()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b8c2a62f-f1ce-4327-8d98-c4e0cc0ebed4");

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(nodeView.ViewModel.NodeModel);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.HomeSpaceViewModel.Annotations.First();
            Assert.IsNotNull(annotation);

            // Index of first node == 4.
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);

            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
               () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
               {
                   RoutedEvent = Mouse.MouseEnterEvent
               })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse enter node should have The highest index.
                Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
                Assert.AreEqual(2, annotation.ZIndex);
            };

            dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                {
                    RoutedEvent = Mouse.MouseLeaveEvent
                })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse leave node should have the old index.
                Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
                Assert.AreEqual(2, annotation.ZIndex);
            };
        }

        [Test]
        public void NodesHaveCorrectLocationsIndpendentOfCulture()
        {
            string openPath = @"core\nodeLocationTest.dyn";
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-AR");
            Open(openPath);

            Assert.AreEqual(1, this.Model.CurrentWorkspace.Nodes.Count());
            var node = this.Model.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("zu-ZA");
            Open(openPath);

            Assert.AreEqual(1, this.Model.CurrentWorkspace.Nodes.Count());
            node = this.Model.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            Open(openPath);

            Assert.AreEqual(1, this.Model.CurrentWorkspace.Nodes.Count());
            node = this.Model.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        [Test]
        public void SettingNodeAsInputOrOutputMarksGraphAsModified()
        {
            Open(@"core\isInput_isOutput\IsInput.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid("c275ece0-2316-4e27-8d51-915257374c1e");

            // Get a reference to the current workspace
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);
            WorkspaceModel ws = nodeViewModel.DynamoViewModel.CurrentSpace;

            // Verify IsSetAsInput is set to true
            Assert.AreEqual(nodeViewModel.IsSetAsInput, true);
            // Verify IsSetAsOutput is set to false
            Assert.AreEqual(nodeViewModel.IsSetAsOutput, false);
            // Verify the graph is not marked as modified
            Assert.AreEqual(ws.HasUnsavedChanges, false);

            // Set IsSetAsInput to false
            nodeViewModel.IsSetAsInput = false;
            // Verify graph is marked as modified
            Assert.AreEqual(nodeViewModel.IsSetAsInput, false);
            Assert.AreEqual(ws.HasUnsavedChanges, true);

            // Save the graph
            string tempPath = Path.Combine(Path.GetTempPath(), "IsInput.dyn");
            ws.Save(tempPath);

            // Verify graph is no longer marked as modified
            Assert.AreEqual(nodeViewModel.IsSetAsInput, false);
            Assert.AreEqual(ws.HasUnsavedChanges, false);

            // Repeat process for IsSetAsOutput //

            // Verify IsSetAsOutput is set to false
            Assert.AreEqual(nodeViewModel.IsSetAsOutput, false);
            // Set IsSetAsOutput to true
            nodeViewModel.IsSetAsOutput = true;

            // Verify graph is marked as modified
            Assert.AreEqual(nodeViewModel.IsSetAsOutput, true);
            Assert.AreEqual(ws.HasUnsavedChanges, true);

            // Save the graph
            ws.Save(tempPath);

            // Verify graph is no longer marked as modified
            Assert.AreEqual(nodeViewModel.IsSetAsOutput, true);
            Assert.AreEqual(ws.HasUnsavedChanges, false);

            // Delete temp file
            File.Delete(tempPath);
        }

        [Test]
        public void SettingOriginalNodeName()
        {
            Open(@"core\originalNodeNameTests\originalNodeName.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid("37749b95-916a-4dfe-b69b-b516b2ccafe9");

            // Get a reference to the current workspace
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            string expectedOriginalName = "List Create";

            Assert.AreEqual(false, nodeViewModel.IsRenamed);
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);

            string newName = "NewName";
            nodeViewModel.Name = newName;
            Assert.AreEqual(nodeViewModel.IsRenamed, true);
            Assert.AreEqual(nodeViewModel.Name, newName);
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);
        }

        [Test]
        public void CheckDummyNodeName()
        {
            Open(@"core\originalNodeNameTests\originalNodeName.dyn");

            // Get the node view for the dummy node in the graph
            NodeView nodeView = NodeViewWithGuid("94e8d6a8-5463-483c-9fe6-ace668670fae");
            // Get a reference to the dummy node view model
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            Assert.AreEqual(false, nodeViewModel.IsRenamed);
            string expectedOriginalName = "Colors.GetAllColors_V1";
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);
        }

        [Test]
        public void CheckNotLoadedCustomNodeOriginalName()
        {
            // Custom node inside is not loaded this case so that renamed tag will be disabled
            Open(@"core\originalNodeNameTests\originalNodeName.dyn");

            // Get the node view for the not loaded custom node in the graph
            NodeView nodeView = NodeViewWithGuid("5795dc19-47c9-4084-a5da-df248e03edc4");
            
            // Get a reference to the custom node view model
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            Assert.AreEqual(false, nodeViewModel.IsRenamed);
            string expectedOriginalName = "RootNode_V1";
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);
        }

        [Test]
        public void CheckLoadedCustomNodeOriginalName()
        {
            // Opening the dyf first will make sure the custom node is loaded
            // so that rename tag can function correctly in this case
            Open(@"core\custom_node_dep_test\RootNode.dyf");
            Open(@"core\originalNodeNameTests\originalNodeName.dyn");

            // Get the node view for the not loaded custom node in the graph
            NodeView nodeView = NodeViewWithGuid("5795dc19-47c9-4084-a5da-df248e03edc4");

            // Get a reference to the custom node view model
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            Assert.AreEqual(true, nodeViewModel.IsRenamed);
            string expectedNewOriginalName = "RootNode";
            Assert.AreEqual(nodeViewModel.OriginalName, expectedNewOriginalName);
        }

        [Test]
        public void SettingOriginalNodeNameOnCustomNode()
        {
            Open(@"core\originalNodeNameTests\originalNodeNameCustomNode.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("d0b81747d16f48cfbc9992182783d229").ToString());

            // Get a reference to the current workspace
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            string expectedOriginalName = "NestedNode";

            Assert.AreEqual(nodeViewModel.IsRenamed, false);
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);

            string newName = "NewName";
            nodeViewModel.Name = newName;
            Assert.AreEqual(nodeViewModel.IsRenamed, true);
            Assert.AreEqual(nodeViewModel.Name, newName);
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);
        }

        [Test]
        [Category("RegressionTests")]
        public void GettingNodeNameDoesNotTriggerPropertyChangeCycle()
        {
            //add a node
            var numNode = new CoreNodeModels.Input.DoubleInput();
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(numNode, true);

            //subscribe to all property changes
            var nvm = ViewModel.CurrentSpaceViewModel.Nodes.First();
            nvm.PropertyChanged += NodeNameTest_PropChangedHandler;
            //get the node name.
            var temp = nvm.Name;
            nvm.PropertyChanged -= NodeNameTest_PropChangedHandler;
        }

        private void NodeNameTest_PropChangedHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //get the name,this will sometimes cause another propertyChanged event
             var temp = (sender as NodeViewModel).Name;
        }
    }
}