using System;
using System.IO;
using System.Linq;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Selection;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using System.Windows.Input;
using System.Globalization;
using System.Threading;

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
    }
}